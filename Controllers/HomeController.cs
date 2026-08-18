using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IETCD.Data;
using IETCD.Models;

namespace IETCD.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Courses)
                .Include(c => c.Tags)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var featuredCourses = await _context.Courses
                .Include(c => c.Category)
                .Where(c => c.IsPublished)
                .OrderByDescending(c => c.CreatedDate)
                .Take(6)
                .ToListAsync();

            var sampleTags = await _context.Tags
                .OrderBy(t => Guid.NewGuid())
                .Take(8)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.FeaturedCourses = featuredCourses;
            ViewBag.SampleTags = sampleTags;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ComingSoon()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}