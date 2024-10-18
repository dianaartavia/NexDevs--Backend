using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class Collection
    {
        [Key]
        public int CollectionId { get; set; }

        public int WorkId { get; set; }

        public string CollectionImageUrl { get; set; }

        public string ImagePublicId { get; set; }

    }
}