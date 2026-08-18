using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IETCD.Data;
using IETCD.Models;

namespace IETCD.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Courses")]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // ADMIN COURSE LIST
        // URL: /Admin/Courses
        // =====================================================
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.CourseTags)
                    .ThenInclude(ct => ct.Tag)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return View("~/Views/Admin/Courses/Index.cshtml", courses);
        }

        // =====================================================
        // CREATE COURSE - GET
        // URL: /Admin/Courses/Create
        // =====================================================
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await LoadCourseData();

            return View("~/Views/Admin/Courses/Create.cshtml");
        }

        // =====================================================
        // CREATE COURSE - POST
        // =====================================================
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Course course,
            int[]? selectedTagIds)
        {
            if (!ModelState.IsValid)
            {
                await LoadCourseData();
                return View("~/Views/Admin/Courses/Create.cshtml", course);
            }

            course.CreatedDate = DateTime.UtcNow;

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            if (selectedTagIds != null)
            {
                foreach (var tagId in selectedTagIds.Distinct())
                {
                    _context.CourseTags.Add(new CourseTag
                    {
                        CourseId = course.Id,
                        TagId = tagId
                    });
                }

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Course created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // EDIT COURSE - GET
        // URL: /Admin/Courses/Edit/5
        // =====================================================
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.Courses
                .Include(c => c.CourseTags)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            await LoadCourseData();

            ViewBag.SelectedTagIds = course.CourseTags
                .Select(ct => ct.TagId)
                .ToList();

            return View("~/Views/Admin/Courses/Edit.cshtml", course);
        }

        // =====================================================
        // EDIT COURSE - POST
        // =====================================================
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Course updatedCourse,
            int[]? selectedTagIds)
        {
            if (id != updatedCourse.Id)
                return BadRequest();

            var course = await _context.Courses
                .Include(c => c.CourseTags)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadCourseData();

                ViewBag.SelectedTagIds = selectedTagIds?.ToList()
                                          ?? new List<int>();

                return View("~/Views/Admin/Courses/Edit.cshtml", updatedCourse);
            }

            course.Title = updatedCourse.Title;
            course.Description = updatedCourse.Description;
            course.ThumbnailUrl = updatedCourse.ThumbnailUrl;
            course.Price = updatedCourse.Price;
            course.InstructorName = updatedCourse.InstructorName;
            course.IsPublished = updatedCourse.IsPublished;
            course.CategoryId = updatedCourse.CategoryId;

            // Remove old tags
            _context.CourseTags.RemoveRange(course.CourseTags);

            // Add new tags
            if (selectedTagIds != null)
            {
                foreach (var tagId in selectedTagIds.Distinct())
                {
                    _context.CourseTags.Add(new CourseTag
                    {
                        CourseId = course.Id,
                        TagId = tagId
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Course updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // DELETE COURSE
        // URL: POST /Admin/Courses/Delete/5
        // =====================================================
        [HttpPost("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses
                .Include(c => c.CourseTags)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                TempData["Error"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            _context.CourseTags.RemoveRange(course.CourseTags);
            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Course deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // PUBLISH / UNPUBLISH
        // URL: POST /Admin/Courses/TogglePublish/5
        // =====================================================
        [HttpPost("TogglePublish/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                TempData["Error"] = "Course not found.";
                return RedirectToAction(nameof(Index));
            }

            course.IsPublished = !course.IsPublished;

            await _context.SaveChangesAsync();

            TempData["Success"] = course.IsPublished
                ? "Course published successfully."
                : "Course unpublished successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // LOAD CATEGORIES + TAGS
        // =====================================================
        private async Task LoadCourseData()
        {
            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Tags = await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
    }
}