using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;

namespace API_Network.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CloudinaryController : ControllerBase
    {
        private readonly string? _cloudinaryUrl;

        public CloudinaryController(IConfiguration configuration)
        {
            _cloudinaryUrl = configuration.GetSection("Cloudinary")["URL"];
        }

        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> SaveImage(IFormFile photo)
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
                System.IO.File.Delete(fileWithPath);

                return Ok(uploadResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("delete")]
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

        [HttpPut]
        [Route("edit")]
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