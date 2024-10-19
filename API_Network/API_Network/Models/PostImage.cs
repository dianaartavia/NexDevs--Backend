using API_Network.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Network.Models
{
    public class PostImage
    {
        [Key]
        public int PostId { get; set; }

        public int WorkId { get; set; }

        public string ContentPost { get; set; }

        public IFormFile PostImageUrl { get; set; }
        
        public DateTime CreateAt { get; set; }

        public int LikesCount { get; set; }

        public int CommentsCount { get; set; }

        public int Approved { get; set; }
    }
}