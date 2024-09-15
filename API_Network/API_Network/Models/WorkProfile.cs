using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class WorkProfile
    {
        [Key]
        public int WorkId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Number { get; set; }

        public string Password { get; set; }

        public string Province { get; set; }

        public string City { get; set; }

        public string WorkDescription { get; set; }

        public string ProfilePictureUrl { get; set; }

        public int CategoryId { get; set; }

        public char ProfileType { get; set; }
       
        public string Salt { get; set; }


    }
}
