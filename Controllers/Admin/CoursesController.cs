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

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
            return View("~/Views/Admin/Courses/Index.cshtml", courses);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Tags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
            return View("~/Views/Admin/Courses/Create.cshtml");
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(Course course, int[] selectedTagIds)
        {
            if (ModelState.IsValid)
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                if (selectedTagIds != null)
                {
                    foreach (var tagId in selectedTagIds)
                    {
                        _context.CourseTags.Add(new CourseTag { CourseId = course.Id, TagId = tagId });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index");
            }

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Tags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
            return View("~/Views/Admin/Courses/Create.cshtml", course);
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.Courses
                .Include(c => c.CourseTags)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Tags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
            ViewBag.SelectedTagIds = course.CourseTags.Select(ct => ct.TagId).ToList();

            return View("~/Views/Admin/Courses/Edit.cshtml", course);
        }

        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, Course updatedCourse, int[] selectedTagIds)
        {
            var course = await _context.Courses
                .Include(c => c.CourseTags)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            course.Title = updatedCourse.Title;
            course.Description = updatedCourse.Description;
            course.ThumbnailUrl = updatedCourse.ThumbnailUrl;
            course.Price = updatedCourse.Price;
            course.InstructorName = updatedCourse.InstructorName;
            course.IsPublished = updatedCourse.IsPublished;
            course.CategoryId = updatedCourse.CategoryId;

            _context.CourseTags.RemoveRange(course.CourseTags);
            if (selectedTagIds != null)
            {
                foreach (var tagId in selectedTagIds)
                {
                    _context.CourseTags.Add(new CourseTag { CourseId = course.Id, TagId = tagId });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}