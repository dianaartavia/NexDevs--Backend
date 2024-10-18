using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Network.Models
{
    public class CommentData
    {
        public int CommentId { get; set; }
        public int PostId { get; set; } 
        public string ContentComment { get; set; }
        public DateTime CreateAt { get; set; }
        public int LikesCount { get; set; }
        public int? WorkId { get; set; }
        public string Name { get; set; }
        public string ProfilePictureUrlWorker { get; set; }
        public string ImagePublicIdWorker { get; set; }
        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePictureUrlUser { get; set; }
        public string ImagePublicIdUser { get; set; }
    }
}