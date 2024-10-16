using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class LikePost
    {
        public int PostId { get; set; }
        public int? UserId { get; set; }
        public int? WorkProfileId { get; set; }
    }
}