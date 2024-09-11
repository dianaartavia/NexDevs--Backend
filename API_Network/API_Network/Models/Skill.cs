using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class Skill
    {
        [Key]
        public int Id { get; set; }

        public string SkillName { get; set; }
    }
}
