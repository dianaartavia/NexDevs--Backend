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
                    var uploadResult = await cloudinary.UploadAsync(uploadParams); // Subir la imagen a Cloudinary
                    if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK) // Verificar si la subida fue exitosa
                    {
                        var publicId = uploadResult.PublicId;// Extraer PublicId y URL de la imagen subida
                        var url = uploadResult.SecureUrl.ToString();
                        return Ok(new { PublicId = publicId, Url = url });// Devolver una respuesta con PublicId y URL
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
    }
}