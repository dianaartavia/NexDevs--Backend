using API_Network.Context;
using API_Network.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Network.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class SkillsController : Controller
    {
        private readonly DbContextNetwork _context;

        public SkillsController(DbContextNetwork sContext)
        {
            _context = sContext;
        }

        ////[Authorize]
        [HttpGet("Listado")]
        public async Task<List<Skill>> Listado()
        {
            var list = await _context.Skills.ToListAsync();

            if (list == null)
            {
                return new List<Skill>();
            }
            else
            {
                return list;
            }
        }

        ////[Authorize]
        [HttpGet("Consultar")]
        public async Task<Skill> Consultar(int skillId)
        {
            var temp = await _context.Skills.FirstOrDefaultAsync(s=>s.Id == skillId);

            return temp;
        }

        //[Authorize]
        [HttpPost("Agregar")]
        public string Agregar(Skill skill)
        {
            string msj = "";
            bool skillExist = _context.Skills.Any(s => s.Id == skill.Id); //verifica si ya hay una Skill con los mismos datos
            try
            {
                if (!skillExist)
                {
                    _context.Skills.Add(skill);
                    _context.SaveChanges();
                    msj = "Skill registrada correctamente";
                }
                else
                {
                    msj = "Esos datos ya existen o son incorrectos";
                }
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }

        //[Authorize]
        [HttpPut("Editar")]
        public string Editar(Skill skill)
        {
            string msj = "";
            bool skillExist = _context.Skills.Any(s => s.SkillName == skill.SkillName);
            try
            {
                if (!skillExist)
                {
                    _context.Skills.Update(skill);
                    _context.SaveChanges();
                    msj = "Skill editada correctamente";
                }
                else
                {
                    msj = "Esos datos ya existen o son incorrectos";
                }
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }

            return msj;
        }

        //[Authorize]
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(int id)
        {
            string msj = "";
            try
            {
                var data = await _context.Skills.FirstOrDefaultAsync(s => s.Id == id);
                var listWorkSkills = await _context.WorkSkills.ToListAsync();
                foreach (var ws in listWorkSkills) //se busca si en la tabla workSkill todos los datos relacionados a la skill y se eliminan
                {
                    if (ws.SkillId == data.Id)
                    {
                        _context.WorkSkills.Remove(ws);
                        _context.SaveChanges();
                    }
                }
                _context.Skills.Remove(data);
                _context.SaveChanges();
                msj = $"La skill: {data.SkillName}, ha sido eliminada correctamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }
    }
}
