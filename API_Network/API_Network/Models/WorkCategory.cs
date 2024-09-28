using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class WorkCategory
    {
        [Key]
        public int Id { get; set; }

        public int WorkId { get; set; }

        public int CategoryId { get; set; }
    }
}