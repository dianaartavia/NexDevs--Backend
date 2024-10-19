using API_Network.Context;
using API_Network.Models;
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
            }//end if/else
        }//end Listado

        //[Authorize]
        [HttpGet("Consultar")]
        public async Task<Category> Consultar(int categoryId)
        {
            var temp = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            return temp;
        }//end consultar 

        //[Authorize]
        [HttpPost("Agregar")]
        public async Task<string> Agregar(CategoryImage category)
        {
            string msj = "";
            //verifica si ya hay un Categorie con los mismos datos
            bool categoryExist = _context.Categories.Any(c => c.CategoryId == category.CategoryId);
            var imageUrl = "";
            var publicId = "";

            try
            {
                if (!categoryExist)
                {
                    if (category.CategoryImageUrl != null)
                    {
                        // Llamar al método de subida de imagen
                        var result = await _cloudinaryController.SaveImage(category.CategoryImageUrl, "categories");

                        if (result is OkObjectResult okResult)
                        {
                            // Extraer los valores de la respuesta
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
        public async Task<string> EditarAsync(CategoryImage category)
        {
            string msj = "Error al editar la categoria";
            var categoryExist = _context.Categories.FirstOrDefault(c => c.CategoryId == category.CategoryId);
            try
            {
                if (category.CategoryImageUrl != null)
                {
                    var tempPublicId = categoryExist.ImagePublicId;

                    var result = await _cloudinaryController.SaveImage(category.CategoryImageUrl, "categories");


                    if (result is OkObjectResult okResult)
                    {
                        var uploadResult = okResult.Value as dynamic;
                        if (uploadResult != null)
                        {
                            await _cloudinaryController.DeleteImage(tempPublicId);
                            categoryExist.CategoryImageUrl = uploadResult.Url;
                            categoryExist.ImagePublicId = uploadResult.PublicId;
                        }
                    }
                }
                else if (category.CategoryImageUrl == null)
                {
                    categoryExist.CategoryImageUrl = "ND";
                    categoryExist.ImagePublicId = "ND";
                }

                categoryExist.CategoryName= category.CategoryName;

                _context.Categories.Update(categoryExist);
                _context.SaveChanges();
                msj = "Categoria editada correctamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end

            return msj;
        }//end Editar

        //[Authorize]
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(int categoryId)
        {
            string msj = "";

            try
            {
                var data = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                if (data != null)
                {
                    //se busca en la tabla WorkCategories si hay alguno con esta categoria
                    var listWorkCategories = await _context.WorkCategories.Where(wc => wc.CategoryId == categoryId).ToListAsync();

                    //Se eliminan todos los que tienen el mismo CategoryId
                    foreach (var wc in listWorkCategories)
                    {
                        _context.WorkCategories.Remove(wc);
                        _context.SaveChanges();

                    }//end foreach

                    //se elimina la imagen de cloudinary
                    await _cloudinaryController.DeleteImage(data.ImagePublicId);

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
        }//end Eliminar

    }
}
