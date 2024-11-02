using API_Network.Context;
using API_Network.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

namespace API_Network.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class CategoriesController : Controller
    {
        private readonly DbContextNetwork _context;
        private readonly CloudinaryController _cloudinaryController;
        public CategoriesController(DbContextNetwork wcContext, CloudinaryController cloudinaryController)
        {
            _context = wcContext;
            _cloudinaryController = cloudinaryController;
        }

        //[Authorize]
        [HttpGet("Listado")]
        public async Task<List<Category>> Listado()
        {
            var list = await _context.Categories.ToListAsync();

            if (list == null)
            {
                return new List<Category>();
            }
            else
            {
                return list;
            }
        }

        //[Authorize]
        [HttpGet("Consultar")]
        public async Task<Category> Consultar(int categoryId)
        {
            var temp = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            return temp;
        }

        [Authorize]
        [HttpPost("Agregar")]
        public async Task<string> Agregar(CategoryImage category)
        {
            string msj = "";
            bool categoryExist = _context.Categories.Any(c => c.CategoryId == category.CategoryId); //verifica si ya hay un Categorie con los mismos datos
            var imageUrl = "";
            var publicId = "";

            try
            {
                if (!categoryExist)
                {
                    if (category.CategoryImageUrl != null)
                    {
                        var result = await _cloudinaryController.SaveImage(category.CategoryImageUrl, "categories"); // Llamar al método de subida de imagen
                        if (result is OkObjectResult okResult)
                        {
                            var uploadResult = okResult.Value as dynamic;

                            if (uploadResult != null)
                            {
                                publicId = uploadResult.PublicId;
                                imageUrl = uploadResult.Url;
                            }
                        }
                    }
                    else if (category.CategoryImageUrl == null)
                    {
                        imageUrl = "ND";
                        publicId = "ND";
                    }

                    var newCategory = new Category
                    {
                        CategoryName = category.CategoryName,
                        CategoryImageUrl = imageUrl,
                        ImagePublicId = publicId
                    };

                    _context.Categories.Add(newCategory);
                    _context.SaveChanges();
                    msj = "Categoria registrada correctamente";
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

        [Authorize]
        [HttpPut("Editar")]
        public async Task<string> Editar(CategoryImage category)
        {
            string msj = "Error al editar la categoria";

            try
            {
                var categoryExist = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId); // Verifica si la categoría existe
                if (categoryExist == null)
                {
                    return "No se encontró la categoría";
                }
                if (category.CategoryImageUrl != null)
                {
                    var tempPublicId = categoryExist.ImagePublicId;
                    var result = await _cloudinaryController.SaveImage(category.CategoryImageUrl, "categories");
                    if (result is OkObjectResult okResult)
                    {
                        var uploadResult = okResult.Value as dynamic;
                        if (uploadResult != null)
                        {
                            await _cloudinaryController.DeleteImage(tempPublicId); // Elimina la imagen anterior de Cloudinary
                            categoryExist.CategoryImageUrl = uploadResult.Url; // Asigna la nueva imagen
                            categoryExist.ImagePublicId = uploadResult.PublicId;
                        }
                    }
                }
                else
                {
                    categoryExist.CategoryImageUrl = categoryExist.CategoryImageUrl ?? "ND";
                    categoryExist.ImagePublicId = categoryExist.ImagePublicId ?? "ND";
                }
                categoryExist.CategoryName = category.CategoryName; // Actualiza el nombre de la categoría
                _context.Categories.Update(categoryExist); // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();
                msj = "Categoría editada correctamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException?.ToString()}";
            }

            return msj;
        }

        [Authorize]
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(int categoryId)
        {
            string msj = "";
            try
            {
                var data = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                if (data != null)
                {
                    var listWorkCategories = await _context.WorkCategories.Where(wc => wc.CategoryId == categoryId).ToListAsync(); //se busca en la tabla WorkCategories si hay alguno con esta categoria
                    foreach (var wc in listWorkCategories) //Se eliminan todos los que tienen el mismo CategoryId
                    {
                        _context.WorkCategories.Remove(wc);
                        _context.SaveChanges();

                    }
                    await _cloudinaryController.DeleteImage(data.ImagePublicId); //se elimina la imagen de cloudinary
                    _context.Categories.Remove(data);
                    _context.SaveChanges();
                    msj = $"La Categoria: {data.CategoryName}, ha sido eliminada correctamente";
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
