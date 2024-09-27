using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class WorkSkill
    {
        [Key]
        public int WorkSkillId { get; set; }

        public int WorkId { get; set; }

        public int SkillId { get; set; }
    }
}