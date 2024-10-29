using API_Network.Models;
using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class ReviewUserData
    {
        public int ReviewId { get; set; }
        public int WorkId { get; set; }
        public string ReviewComment { get; set; }
        public DateTime CreateAt { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePictureUrlUser { get; set; }
        public string ImagePublicId { get; set; }
    }
}