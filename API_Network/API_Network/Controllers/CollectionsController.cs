using API_Network.Context;
using API_Network.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Network.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class CollectionsController : Controller
    {
        private readonly DbContextNetwork _context;

        public CollectionsController(DbContextNetwork cContext)
        {
            _context = cContext;
        }

        //[Authorize]
        [HttpGet("Listado")]
        public async Task<List<Collection>> Listado()
        {
            var list = await _context.Collections.ToListAsync();

            if (list == null)
            {
                return new List<Collection>();
            }
            else
            {
                return list;
            }//end if/else
        }//end Listado

        //[Authorize]
        [HttpGet("Consultar")]
        public async Task<List<Collection>> Consultar(int workId)
        {
            var temp = await _context.Collections.Where(c => c.WorkId == workId).ToListAsync();
            return temp;
        }//end Consultar

        //[Authorize]
        [HttpPost("Agregar")]
        public string Agregar(Collection collection)
        {
            string msj = "";
            //verifica que el workId exista
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == collection.WorkId);

            try
            {
                if (workExist)
                {
                    _context.Collections.Add(collection);
                    _context.SaveChanges();
                    msj = "Collection registrado correctamente";
                }
                else
                {
                    msj = $"El workId {collection.WorkId}, no existe";
                }

            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end

            return msj;
        }//end Agregar

        //[Authorize]
        [HttpPut("Editar")]
        public string Editar(Collection collection)
        {
            string msj = "";

            //verifica que el workId exista
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == collection.WorkId);

            try
            {
                if (workExist)
                {
                    _context.Collections.Update(collection);
                    _context.SaveChanges();
                    msj = "Collection editado correctamente";
                }
                else
                {
                    msj = $"El workId {collection.WorkId}, no existe";
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
                var temp = await _context.Collections.FirstOrDefaultAsync(c => c.CollectionId == id);
                if (temp == null)
                {
                    msj = "No existe ningun collection con el ID " + id;
                }
                else
                {
                    _context.Collections.Remove(temp);
                    await _context.SaveChangesAsync();
                    msj = $"Collection con el ID {temp.CollectionId}, eliminado correctamente";
                }//end else
            }
            catch (Exception ex) 
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }//end Eliminar
        
    }
}//end namespace