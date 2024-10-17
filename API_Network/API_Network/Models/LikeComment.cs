using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class LikeComment
    {
        public int CommentId { get; set; }
        public int? UserId { get; set; }
        public int? WorkProfileId { get; set; }
    }
}