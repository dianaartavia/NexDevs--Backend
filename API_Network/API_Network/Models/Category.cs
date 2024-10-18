using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CategoryImageUrl { get; set; }

        public string ImagePublicId { get; set; }
    }
}