using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;

namespace API_Network.Controllers
{
    // [ApiController]
    // [Route("api/[controller]")]
    public class CloudinaryController : ControllerBase
    {
        private readonly string? _cloudinaryUrl;

        public CloudinaryController(IConfiguration configuration)
        {
            _cloudinaryUrl = configuration.GetSection("Cloudinary")["URL"];
        }

        // [HttpPost]
        // [Route("save")]
        public async Task<IActionResult> SaveImage(IFormFile photo, string folder)
        {
            try
            {
                Cloudinary cloudinary = new Cloudinary(_cloudinaryUrl);

                // Asegurarse de que el archivo ha sido recibido correctamente
                if (photo == null || photo.Length == 0)
                {
                    return BadRequest("No se ha subido ninguna imagen.");
                }

                // Subir la imagen directamente desde el archivo en memoria sin guardarla localmente
                using (var stream = photo.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(photo.FileName, stream),
                        UseFilename = true,
                        Overwrite = true,
                        Folder = folder
                    };


                    // Subir la imagen a Cloudinary
                    var uploadResult = await cloudinary.UploadAsync(uploadParams);

                    // Verificar si la subida fue exitosa
                    if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // Extraer PublicId y URL de la imagen subida
                        var publicId = uploadResult.PublicId;
                        var url = uploadResult.SecureUrl.ToString();

                        // Devolver una respuesta con PublicId y URL
                        return Ok(new { PublicId = publicId, Url = url });
                    }
                    else
                    {
                        return BadRequest("Error al subir la imagen a Cloudinary.");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // [HttpDelete]
        // [Route("delete")]
        public async Task<IActionResult> DeleteImage(string publicId)
        {
            try
            {
                Cloudinary cloudinary = new Cloudinary(_cloudinaryUrl);
                var deleteParams = new DeletionParams(publicId);
                var result = await cloudinary.DestroyAsync(deleteParams);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // [HttpPut]
        // [Route("edit")]
        //public async Task<IActionResult> EditImage(string publicId, IFormFile photo, string folder)
        //{
        //    try
        //    {
        //        Cloudinary cloudinary = new Cloudinary(_cloudinaryUrl);

        //        // Verificar si el archivo es nulo o tiene un tamaño inválido
        //        if (photo == null || photo.Length == 0)
        //        {
        //            return BadRequest("No se proporcionó una imagen válida.");
        //        }

        //        // Eliminar la imagen anterior de Cloudinary
        //        var deleteParams = new DeletionParams(publicId);
        //        var deleteResult = await cloudinary.DestroyAsync(deleteParams);
        //        if (deleteResult.Result != "ok")
        //        {
        //            return BadRequest("No se pudo eliminar la imagen anterior.");
        //        }

        //        // Subir la nueva imagen
        //        var uploadResult = await this.SaveImage(photo, folder);
        //        return Ok(uploadResult);

        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}