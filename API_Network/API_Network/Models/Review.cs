using API_Network.Models;
using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        public int UserId { get; set; }

        public int WorkId { get; set; }

        public string ReviewComment { get; set; }

        public DateTime CreateAt { get; set; }
    }
}