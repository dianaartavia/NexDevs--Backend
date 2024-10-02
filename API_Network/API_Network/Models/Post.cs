using API_Network.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Network.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        public int WorkId { get; set; }

        public string ContentPost { get; set; }

        public string PostImageUrl { get; set; }

        public DateTime CreateAt { get; set; }

        public int LikesCount { get; set; }

        public int CommentsCount { get; set; }

        public int Approved { get; set; }

        [ForeignKey("WorkId")]
        public WorkProfile WorkProfile { get; set; }
    }
}