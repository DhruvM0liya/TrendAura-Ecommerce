using Mobile_Store.Models;

namespace Mobile_Store.ViewModels
{
    public class AdminReviewsViewModel
    {
        public List<Review> Reviews { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        public int? SelectedProductId { get; set; }
        public int TotalCount { get; set; }
    }
}
