using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class WorkCategorie
    {
        [Key]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CategoryImageUrl { get; set; }
    }
}