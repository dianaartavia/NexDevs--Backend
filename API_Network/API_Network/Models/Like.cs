using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class Like
    {
        [Key]
        public int Id { get; set; }
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
        public int? UserId { get; set; }
        public int? WorkProfileId { get; set; }

        public virtual Post Post { get; set; }
        public virtual Comment Comment { get; set; }
    }
}