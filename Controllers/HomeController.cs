using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using trendaura.Data;
using trendaura.Models;
using trendaura.ViewModels; // Model mismatch error fix karne ke liye zaroori hai

namespace trendaura.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // Featured products loading logic
            var featuredProducts = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToListAsync();

            ViewBag.Categories = await _db.Categories.ToListAsync();

            return View(featuredProducts);
        }

        public IActionResult About()
        {
            return View();
        }

        public async Task<IActionResult> Category(int id)
        {
            var products = await _db.Products
                .Where(p => p.CategoryId == id)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .ToListAsync();

            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Search(string q)
        {
            var products = await _db.Products
                .Where(p => p.Name.Contains(q))
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .ToListAsync();

            ViewBag.Query = q;
            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            // Product ko include karte hain uski category aur AccessoryReviews ke saath
            var product = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews) // Yahan check karein agar model mein iska naam AccessoryReviews hai
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            // FIXED: Correcting property names to match your DB (AccessoryReview)
            var viewModel = new ProductReviewsViewModel
            {
                Product = product,
                // Yahan AccessoryReview use karna hai
                Reviews = product.Reviews?.ToList() ?? new List<AccessoryReview>(),
                NewReview = new AccessoryReview { AccessoryId = id }
            };

            return View(viewModel);
        }
    }
}