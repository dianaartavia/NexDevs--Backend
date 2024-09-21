using API_Network.Context;
using API_Network.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Network.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class WorkSkillsController : Controller
    {
        private readonly DbContextNetwork _context;

        public WorkSkillsController(DbContextNetwork wsContext) //ws in wsContext = work skills
        {
            _context = wsContext;
        }

        //[Authorize]
        [HttpGet("Listado")]
        public async Task<List<WorkSkill>> Listado()
        {
            var list = await _context.WorkSkills.ToListAsync();

            if (list == null)
            {
                return new List<WorkSkill>();
            }
            else
            {
                return list;
            }//end if/else
        }//end Listado

        //[Authorize]
        [HttpGet("Consultar")]
        public async Task<List<WorkSkill>> Consultar(int workId)
        {
            var temp = await _context.WorkSkills.Where(ws => ws.WorkId == workId).ToListAsync();

            return temp;
        }//end consultar por workId


        //Agregue esta para que en una sola consulta tambien retorne los nombres de las skills y no solo sus ids. - Luis
        //[Authorize]
        [HttpGet("ConsultarID")]
        public async Task<ActionResult<List<WorkSkillDto>>> ConsultarID(int workId)
        {
            var temp = await _context.WorkSkills
                .Where(ws => ws.WorkId == workId)
                .Include(ws => ws.Skill)
                .Select(ws => new WorkSkillDto
                {
                    WorkId = ws.WorkId,
                    SkillId = ws.SkillId,
                    SkillName = ws.Skill.SkillName
                })
                .ToListAsync();

            if (temp == null || !temp.Any())
            {
                return NotFound();
            }
            return Ok(temp);
        }//end

        //[Authorize]
        [HttpPost("Agregar")]
        public string Agregar(WorkSkill workSkill)
        {
            string msj = "";

            //verifica si ya hay un WorkSkill con los mismos datos
            bool workSkillExist = _context.WorkSkills.Any(ws => ws.WorkId == workSkill.WorkId && ws.SkillId == workSkill.SkillId);

            //verifica que el workId exista
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == workSkill.WorkId);

            //verifica que el skillId exista
            bool skillExiste = _context.Skills.Any(s => s.Id == workSkill.SkillId);

            try
            {
                if (!workSkillExist && workExist && skillExiste)
                {
                    _context.WorkSkills.Add(workSkill);
                    _context.SaveChanges();
                    msj = "WorkSkill registrada correctamente";
                }
                else
                {
                    msj = "Esos datos ya existen o son incorrectos";
                }//end else
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end

            return msj;
        }//end Agregar

        //[Authorize]
        [HttpPut("Editar")]
        public string Editar(WorkSkill workSkill) 
        {
            string msj = "";

            //verifica si ya hay un WorkSkill con los mismos datos
            bool workSkillExist = _context.WorkSkills.Any(ws => ws.WorkId == workSkill.WorkId && ws.SkillId == workSkill.SkillId);

            //verifica que el workId exista
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == workSkill.WorkId);

            //verifica que el skillId exista
            bool skillExiste = _context.Skills.Any(s => s.Id == workSkill.SkillId);

            try
            {
                if (!workSkillExist && workExist && skillExiste)
                {
                    _context.WorkSkills.Update(workSkill);
                    _context.SaveChanges();
                    msj = "WorkSkill editado correctamente";
                }
                else
                {
                    msj = "Esos datos ya existen o son incorrectos";
                }//end else
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end

            return msj;
        }//end Editar

        //[Authorize]
        [HttpDelete("Eliminar")]
        public async Task<string>Eliminar(int id)
        {
            string msj = "";

            try
            {
                var temp = await _context.WorkSkills.FirstOrDefaultAsync(ws => ws.WorkSkillId == id);
                if (temp == null)
                {
                    msj = "No existe ninguna workSkill con el ID " + id;
                }
                else
                {
                    _context.WorkSkills.Remove(temp);
                    await _context.SaveChangesAsync();
                    msj = $"WorkSkill con el ID {temp.WorkSkillId}, eliminado correctamente";
                }//end else
            }
            catch (Exception ex) 
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }//end Eliminar

    }//end class
}//end namespace

//Un modelo para el consultar con el skillname incluido
public class WorkSkillDto
{
    public int WorkId { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; }
}