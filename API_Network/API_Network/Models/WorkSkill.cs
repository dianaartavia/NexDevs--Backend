using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class WorkSkill
    {
        [Key]
        public int WorkSkillId { get; set; }

        public int WorkId { get; set; }

        public int SkillId { get; set; }

        //Las conexiones con Skill para poder retornar tambien sus nombres - Luis
        public virtual Skill Skill { get; set; }

    }
}