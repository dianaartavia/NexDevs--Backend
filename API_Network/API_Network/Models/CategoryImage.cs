using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class CategoryImage
    {
        [Key]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public IFormFile CategoryImageUrl { get; set; }
    }
}