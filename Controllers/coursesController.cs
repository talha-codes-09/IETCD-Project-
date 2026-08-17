using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IETCD.Data;

namespace IETCD.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? categoryId)
        {
            var categories = await _context.Categories
                .Include(c => c.Tags)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var coursesQuery = _context.Courses
                .Include(c => c.Category)
                .Where(c => c.IsPublished)
                .AsQueryable();

            if (categoryId.HasValue)
                coursesQuery = coursesQuery.Where(c => c.CategoryId == categoryId.Value);

            var courses = await coursesQuery.OrderByDescending(c => c.CreatedDate).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;

            return View(courses);
        }

        [HttpGet("Courses/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.CourseTags)
                    .ThenInclude(ct => ct.Tag)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsPublished);

            if (course == null) return NotFound();

            return View(course);
        }
    }
}