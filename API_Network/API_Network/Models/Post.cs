using API_Network.Models;
using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        public int CategoryId { get; set; }

        public string ContentPost { get; set; }

        public string PostImageUrl { get; set; }

        public DateTime CreateAt { get; set; }

        public int LikesCount { get; set; }

        public int CommentsCount { get; set; }
    }
}