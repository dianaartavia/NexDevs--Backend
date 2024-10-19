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
        private readonly CloudinaryController _cloudinaryController;

        public CollectionsController(DbContextNetwork cContext, CloudinaryController cloudinaryController)
        {
            _context = cContext;
            _cloudinaryController = cloudinaryController;
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
        [HttpGet("ConsultarId")]
        public async Task<Collection> ConsultarCollectionId(int collectionId)
        {
            var temp = await _context.Collections.FirstOrDefaultAsync(c => c.CollectionId == collectionId);
            return temp;
        }//end Consultar

        //[Authorize]
        [HttpPost("Agregar")]
        public async Task<string> Agregar(CollectionImage collection)
        {
            string msj = "";
            //verifica que el workId exista
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == collection.WorkId);
            var imageUrl = "";
            var publicId = "";

            try
            {
                if (workExist)
                {
                    if (collection.CollectionImageUrl != null)
                    {
                        // Llamar al método de subida de imagen
                        var result = await _cloudinaryController.SaveImage(collection.CollectionImageUrl, "collection");

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
                    else if (collection.CollectionImageUrl == null)
                    {
                        imageUrl = "ND";
                        publicId = "ND";
                    }

                    var newCollection = new Collection
                    {
                        WorkId = collection.WorkId,
                        CollectionImageUrl = imageUrl,
                        ImagePublicId = publicId
                    };

                    _context.Collections.Add(newCollection);
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
        public async Task<string> EditarAsync(CollectionImage collection)
        {
            string msj = "Error al editar";
            var collectionExist = _context.Collections.FirstOrDefault(c => c.CollectionId == collection.CollectionId);

            //verifica que el workId exista
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == collection.WorkId);

            try
            {
                if (collection.CollectionImageUrl != null)
                {
                    var tempPublicId = collectionExist.ImagePublicId;

                    var result = await _cloudinaryController.SaveImage(collection.CollectionImageUrl, "collection");


                    if (result is OkObjectResult okResult)
                    {
                        var uploadResult = okResult.Value as dynamic;
                        if (uploadResult != null)
                        {
                            await _cloudinaryController.DeleteImage(tempPublicId);
                            collectionExist.CollectionImageUrl = uploadResult.Url;
                            collectionExist.ImagePublicId = uploadResult.PublicId;
                        }
                    }
                }
                else if (collection.CollectionImageUrl == null)
                {
                    collectionExist.CollectionImageUrl = "ND";
                    collectionExist.ImagePublicId = "ND";
                }

                collectionExist.WorkId = collection.WorkId;

                if (workExist)
                {
                    _context.Collections.Update(collectionExist);
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
        public async Task<string> Eliminar(int id)
        {
            string msj = "";

            try
            {
                var data = await _context.Collections.FirstOrDefaultAsync(c => c.CollectionId == id);
                if (data == null)
                {
                    msj = "No existe ningun collection con el ID " + id;
                }
                else
                {
                    //se elimina la imagen de cloudinary
                    await _cloudinaryController.DeleteImage(data.ImagePublicId);

                    _context.Collections.Remove(data);
                    await _context.SaveChangesAsync();
                    msj = $"Collection con el ID {data.CollectionId}, eliminado correctamente";
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