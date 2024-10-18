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
        public async Task<IActionResult> SaveImage(IFormFile photo)
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
                        File = new FileDescription(photo.FileName, stream), // Subir desde el stream del archivo
                        UseFilename = true,
                        Overwrite = true,
                        Folder = "users"
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
        public async Task<IActionResult> EditImage(string publicId, IFormFile photo)
        {
            try
            {
                Cloudinary cloudinary = new Cloudinary(_cloudinaryUrl);
                var fileName = photo.FileName;
                var fileWithPath = Path.Combine("Uploads", fileName);

                using (var stream = new FileStream(fileWithPath, FileMode.Create))
                {
                    photo.CopyTo(stream);
                }

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(fileWithPath),
                    UseFilename = true,
                    Overwrite = true,
                    Folder = "users"
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                var deleteParams = new DeletionParams(publicId);
                await cloudinary.DestroyAsync(deleteParams);

                System.IO.File.Delete(fileWithPath);
                return Ok(uploadResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}