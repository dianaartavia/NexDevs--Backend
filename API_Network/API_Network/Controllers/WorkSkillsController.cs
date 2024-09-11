using API_Network.Context;
using API_Network.Models;
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

        
    }//end class
}//end namespace
