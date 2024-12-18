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

        ////[Authorize]
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
            }
        }

        ////[Authorize]
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
                    id = wc.Id,
                    WorkId = wc.WorkId,
                    CategoryId = wc.CategoryId,
                    CategoryName = categoryName.CategoryName
                });
            }
            return Ok(workCategoryDtos);
        }

        ////[Authorize]
        [HttpGet("ConsultarCategory")]
        public async Task<ActionResult<List<WorkCategoryDto>>> ConsultarCategory(int categoryId)
        {
            // Obtener los trabajadores asociados a la categoria
            var workCategories = await _context.WorkCategories
                                            .Where(c => c.CategoryId == categoryId)
                                            .ToListAsync();

            var workCategory = new List<CategoryWithWorker>();
            foreach (var wc in workCategories)
            {
                var categoryName = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == wc.CategoryId);
                var worker = await _context.WorkProfiles.FirstOrDefaultAsync(w => w.WorkId == wc.WorkId);
                workCategory.Add(new CategoryWithWorker
                {
                    id = wc.Id,
                    CategoryId = wc.CategoryId,
                    CategoryName = categoryName.CategoryName,
                    WorkId = wc.WorkId,
                    Name = worker.Name,
                    Number = worker.Number,
                    Province = worker.Province,
                    City = worker.City,
                    WorkDescription = worker.WorkDescription,
                    ProfilePictureUrl = worker.ProfilePictureUrl,
                    ImagePublicId = worker.ImagePublicId
                });
            }
            return Ok(workCategory);
        }

        ////[Authorize]
        [HttpGet("ConsultarId")]
        public async Task<ActionResult<WorkCategory>> ConsultarPorId(int id)
        {
            var temp = await _context.WorkCategories.FirstOrDefaultAsync(wc => wc.Id == id);
            return temp;
        }

        //[Authorize]
        [HttpPost("Agregar")]
        public string Agregar(WorkCategory workCategory)
        {
            string msj = "";
            bool workCategoryExist = _context.WorkCategories.Any(wc => wc.WorkId == workCategory.WorkId && wc.CategoryId == workCategory.CategoryId); //verifica si ya hay un workCategory con los mismos datos
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == workCategory.WorkId); //verifica que el workId exista
            bool categoryExiste = _context.Categories.Any(c => c.CategoryId == workCategory.CategoryId); //verifica que el categoryId exista
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
        public string Editar(WorkCategory workCategory)
        {
            string msj = "";
            bool workCategoryExist = _context.WorkCategories.Any(wc => wc.WorkId == workCategory.WorkId && wc.CategoryId == workCategory.CategoryId); //verifica si ya hay un WorkCategorie con los mismos datos
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == workCategory.WorkId); //verifica que el workId exista
            bool categoryExist = _context.Categories.Any(c => c.CategoryId == workCategory.CategoryId); //verifica que la category exista
            try
            {
                if (!workCategoryExist && workExist && categoryExist)
                {
                    _context.WorkCategories.Update(workCategory);
                    _context.SaveChanges();
                    msj = "WorkCategory editado correctamente";
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
                var temp = await _context.WorkCategories.FirstOrDefaultAsync(wc => wc.Id == id);
                if (temp == null)
                {
                    msj = $"No existe ninguna workCategory con el Id {temp.Id}";
                }
                else
                {
                    _context.WorkCategories.Remove(temp);
                    await _context.SaveChangesAsync();
                    msj = $"WorkCategory con el Id {temp.Id}, eliminado correctamente";
                }
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }
    }
}

//Un modelo para el consultar con el categoryName incluido
public class WorkCategoryDto
{
    public int id { get; set; }
    public int WorkId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
}

public class CategoryWithWorker
{
    public int id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public int WorkId { get; set; }
    public string Name { get; set; }
    public string Number { get; set; }
    public string Province { get; set; }
    public string City { get; set; }
    public string WorkDescription { get; set; }
    public string ProfilePictureUrl { get; set; }
    public string ImagePublicId { get; set; }

}