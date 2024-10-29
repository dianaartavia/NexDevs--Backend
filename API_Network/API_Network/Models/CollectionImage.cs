using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class CollectionImage
    {
        [Key]
        public int CollectionId { get; set; }
        public int WorkId { get; set; }
        public IFormFile CollectionImageUrl { get; set; }
    }
}