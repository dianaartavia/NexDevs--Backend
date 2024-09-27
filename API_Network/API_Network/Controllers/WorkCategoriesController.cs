using API_Network.Context;
using API_Network.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Network.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class WorkCategoriesController : Controller
    {
        private readonly DbContextNetwork _context;

        public WorkCategoriesController(DbContextNetwork wcContext)
        {
            _context = wcContext;
        }

        //[Authorize]
        [HttpGet("Listado")]
        public async Task<List<WorkCategory>> Listado()
        {
            var list = await _context.WorkCategories.ToListAsync();

            if (list == null)
            {
                return new List<WorkCategory>();
            }
            else
            {
                return list;
            }//end if/else
        }//end Listado

        //[Authorize]
        [HttpGet("Consultar")]
        public async Task<ActionResult<List<WorkCategoryDto>>> Consultar(int workId)
        {
            // Obtener las categorías asociadas al workId
            var workCategories = await _context.WorkCategories
                                               .Where(wc => wc.WorkId == workId)
                                               .ToListAsync();

            var workCategoryDtos = new List<WorkCategoryDto>();

            foreach (var wc in workCategories)
            {
                var categoryName = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == wc.CategoryId);

                workCategoryDtos.Add(new WorkCategoryDto
                {
                    WorkId = wc.WorkId,
                    CategoryId = wc.CategoryId,
                    CategoryName = categoryName.CategoryName 
                });
            }

            return Ok(workCategoryDtos);
        }


        //[Authorize]
        [HttpPost("Agregar")]
        public string Agregar(WorkCategory workCategory)
        {
            string msj = "";

            //verifica si ya hay un workCategory con los mismos datos
            bool workCategoryExist = _context.WorkCategories.Any(wc => wc.WorkId == workCategory.WorkId && wc.CategoryId == workCategory.CategoryId);

            //verifica que el workId exista
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == workCategory.WorkId);

            //verifica que el categoryId exista
            bool categoryExiste = _context.Categories.Any(c => c.CategoryId == workCategory.CategoryId);

            try
            {
                if (!workCategoryExist && workExist && categoryExiste)
                {
                    _context.WorkCategories.Add(workCategory);
                    _context.SaveChanges();
                    msj = "WorkCategory registrada correctamente";
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

        // //[Authorize]
        // [HttpPut("Editar")]
        // public string Editar(WorkSkill workSkill) 
        // {
        //     string msj = "";

        //     //verifica si ya hay un WorkSkill con los mismos datos
        //     bool workSkillExist = _context.WorkSkills.Any(ws => ws.WorkId == workSkill.WorkId && ws.SkillId == workSkill.SkillId);

        //     //verifica que el workId exista
        //     bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == workSkill.WorkId);

        //     //verifica que el skillId exista
        //     bool skillExiste = _context.Skills.Any(s => s.Id == workSkill.SkillId);

        //     try
        //     {
        //         if (!workSkillExist && workExist && skillExiste)
        //         {
        //             _context.WorkSkills.Update(workSkill);
        //             _context.SaveChanges();
        //             msj = "WorkSkill editado correctamente";
        //         }
        //         else
        //         {
        //             msj = "Esos datos ya existen o son incorrectos";
        //         }//end else
        //     }
        //     catch (Exception ex)
        //     {
        //         msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
        //     }//end

        //     return msj;
        // }//end Editar

        // //[Authorize]
        // [HttpDelete("Eliminar")]
        // public async Task<string>Eliminar(int id)
        // {
        //     string msj = "";

        //     try
        //     {
        //         var temp = await _context.WorkSkills.FirstOrDefaultAsync(ws => ws.WorkSkillId == id);
        //         if (temp == null)
        //         {
        //             msj = "No existe ninguna workSkill con el ID " + id;
        //         }
        //         else
        //         {
        //             _context.WorkSkills.Remove(temp);
        //             await _context.SaveChangesAsync();
        //             msj = $"WorkSkill con el ID {temp.WorkSkillId}, eliminado correctamente";
        //         }//end else
        //     }
        //     catch (Exception ex) 
        //     {
        //         msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
        //     }
        //     return msj;
        // }//end Eliminar

    }//end class
}//end namespace

//Un modelo para el consultar con el categoryName incluido
public class WorkCategoryDto
{
    public int WorkId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
}