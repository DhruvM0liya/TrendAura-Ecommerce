using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using trendaura.Data;
using trendaura.Models;

namespace trendaura.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.Categories.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid) return View(category);
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            TempData["success"] = "Category created.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (!ModelState.IsValid) return View(category);
            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
            TempData["success"] = "Category updated.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (cat != null)
            {
                try
                {
                    // Get all products in this category
                    var productIds = cat.Products?.Select(p => p.Id).ToList() ?? new List<int>();
                    
                    if (productIds.Any())
                    {
                        // Delete all cart items referencing these products
                        var cartItemsToDelete = await _db.CartItems
                            .Where(ci => productIds.Contains(ci.ProductId))
                            .ToListAsync();
                        _db.CartItems.RemoveRange(cartItemsToDelete);
                        
                        // Delete all wishlist items referencing these products
                        var wishlistItemsToDelete = await _db.Wishlists
                            .Where(w => productIds.Contains(w.ProductId))
                            .ToListAsync();
                        _db.Wishlists.RemoveRange(wishlistItemsToDelete);
                        
                        // Delete all reviews for these products
                        var reviewsToDelete = await _db.Reviews
                            .Where(r => productIds.Contains(r.ProductId))
                            .ToListAsync();
                        _db.Reviews.RemoveRange(reviewsToDelete);
                    }
                    
                    // Now delete the category and its products
                    _db.Categories.Remove(cat);
                    await _db.SaveChangesAsync();
                    TempData["success"] = "Category and related products deleted successfully.";
                }
                catch (Exception ex)
                {
                    TempData["error"] = $"Error deleting category: {ex.Message}";
                }
            }
            return RedirectToAction("Index");
        }
    }
}