using API_Network.Context;
using API_Network.Models;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace API_Network.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class PostsController : Controller
    {
        private readonly DbContextNetwork _context;
        private readonly Cloudinary _cloudinary;

        public PostsController(IConfiguration configuration, DbContextNetwork pContext)
        {
            var cloudinaryUrl = configuration.GetSection("Cloudinary").GetSection("URL").Value;
            var uri = new Uri(cloudinaryUrl);

            var cloudName = uri.Host; // Cloud Name
            var apiKey = uri.UserInfo.Split(':')[0]; // API Key
            var apiSecret = uri.UserInfo.Split(':')[1]; // API Secret

            // Inicializar Cloudinary con la cuenta
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);

            _context = pContext;
        }

        //[Authorize]
        [HttpGet("ListadoGeneral")]
        public async Task<List<Post>> Listado()
        {
            //var list = await _context.Posts.ToListAsync();
            var list = await _context.Posts
                .Include(p => p.WorkProfile)
                .Select(p => new Post
                {
                    PostId = p.PostId,
                    WorkId = p.WorkId,
                    ContentPost = p.ContentPost,
                    PostImageUrl = p.PostImageUrl,
                    CreateAt = p.CreateAt,
                    LikesCount = p.LikesCount,
                    CommentsCount = p.CommentsCount,
                    Approved = p.Approved,
                    WorkProfile = new WorkProfile
                    {
                        WorkId = p.WorkProfile.WorkId,
                        Name = p.WorkProfile.Name,
                        Email = p.WorkProfile.Email,
                        Number = p.WorkProfile.Number,
                        Password = p.WorkProfile.Password,
                        Province = p.WorkProfile.Province,
                        City = p.WorkProfile.City,
                        WorkDescription = p.WorkProfile.WorkDescription,
                        ProfilePictureUrl = p.WorkProfile.ProfilePictureUrl,
                        ProfileType = p.WorkProfile.ProfileType,
                        Salt = p.WorkProfile.Salt
                    }
                })
                .ToListAsync();

            if (list == null)
            {
                return new List<Post>();
            }
            else
            {
                return list;
            }//end if/else
        }//end Listado

        //[Authorize]
        [HttpGet("Aprobados")]
        public async Task<List<Post>> ListaAprobados()
        {
            var list = await _context.Posts
                                            .Where(p => p.Approved == 1)
                                            .ToListAsync();

            if (list == null)
            {
                return new List<Post>();
            }
            else
            {
                return list;
            }//end if/else
        }//end Listado

        //[Authorize]
        [HttpGet("PorAprobar")]
        public async Task<List<Post>> ListaPendientes()
        {
            var list = await _context.Posts
                                            .Where(p => p.Approved == 0)
                                            .ToListAsync();

            if (list == null)
            {
                return new List<Post>();
            }
            else
            {
                return list;
            }//end if/else
        }//end Listado

        //[Authorize]
        [HttpGet("ConsultarWorkId")]
        public async Task<ActionResult<List<Post>>> ConsultarWorkId(int workId)
        {
            var posts = await _context.Posts
                             .Where(ws => ws.WorkId == workId)
                             .Include(p => p.WorkProfile) // Incluye WorkProfile
                             .ToListAsync();

            return posts;
        }//end

        //[Authorize]
        [HttpGet("Consultar")]
        public async Task<Post> Consultar(int postId)
        {
            var temp = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);

            return temp;
        }//end Consultar

        //Authorize]
        [HttpPost("Like")]
        public async Task<string> Like(int postId)
        {
            var msj = "";
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);

            post.LikesCount += 1;

            try
            {
                _context.Posts.Update(post);
                _context.SaveChanges();
                msj = "Like registrado correctamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end
            return msj;
        }//end Like

        //Authorize]
        [HttpPost("Dislike")]
        public async Task<string> Dislike(int postId)
        {
            var msj = "";
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);

            post.LikesCount = post.LikesCount - 1;

            try
            {
                _context.Posts.Update(post);
                _context.SaveChanges();
                msj = "Dislike registrado correctamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end
            return msj;
        }//end Dislike

        //[Authorize]
        [HttpPost("Agregar")]
        public async Task<ActionResult<string>> Agregar(IFormFile photo, [FromForm] Post post)
        {
            string msj = "";

            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == post.WorkId);

            if (!workExist)
            {
                return BadRequest("El WorkId no existe.");
            }

            try
            {
                // validar si hay imagen
                if (photo != null)
                {
                    var fileName = photo.FileName;
                    var fileWithPath = Path.Combine("Uploads", fileName);

                    // guardar temporalmente el archivo en el servidor
                    using (var stream = new FileStream(fileWithPath, FileMode.Create))
                    {
                        await photo.CopyToAsync(stream);
                    }

                    // configurar los parámetros para subir la imagen a Cloudinary
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(fileWithPath),
                        UseFilename = true,
                        Overwrite = true,
                        Folder = "posts" // carpeta en Cloudinary dedicada a las imágenes de los posts
                    };

                    // subir la imagen a Cloudinary
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    // eliminar el archivo temporal después de la subida
                    System.IO.File.Delete(fileWithPath);

                    // guardar la URL de la imagen en el post
                    post.PostImageUrl = uploadResult.SecureUrl.ToString();
                }

                post.Approved = 0;
                post.CreateAt = DateTime.UtcNow;

                // se guarda el post ya con la URL de la imagen
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                msj = "Post registrado correctamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException?.Message}";
                return StatusCode(500, msj);
            }

            return Ok(msj);
        }

        //[Authorize]
        [HttpPut("Editar")]
        public string Editar(Post post)
        {
            string msj = "";

            //verifica que el workId exista
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == post.WorkId);

            try
            {
                if (workExist)
                {
                    _context.Posts.Update(post);
                    _context.SaveChanges();
                    msj = "Post editado correctamente";
                }
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end

            return msj;
        }//end Editar

        //[Authorize]
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(int postId)
        {
            string msj;

            try
            {
                // Buscar el post en la base de datos
                var postToDelete = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);

                if (postToDelete == null)
                {
                    return $"No existe ningún post con el ID: {postId}";
                }

                // Extraer el public_id de la URL de la imagen
                var publicId = GetPublicIdFromUrl(postToDelete.PostImageUrl);
                

                // Eliminar la imagen de Cloudinary
                var deleteParams = new DeletionParams(publicId);
                var deleteResult = await _cloudinary.DestroyAsync(deleteParams);

                // Verificar si la eliminación fue exitosa
                if (deleteResult.StatusCode == HttpStatusCode.OK)
                {
                    // Si la imagen fue eliminada correctamente, eliminar el post de la base de datos
                    _context.Posts.Remove(postToDelete);
                    await _context.SaveChangesAsync();
                    msj = $"{publicId} Post con el ID {postToDelete.PostId} eliminado correctamente." ;
                }
                else
                {

                    msj = "Error al eliminar la imagen en Cloudinary: " + deleteResult.Error?.Message;
                }
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException?.Message}";
            }

            return msj;
        }

        private string GetPublicIdFromUrl(string imageUrl)
        {
            var uri = new Uri(imageUrl);
            var segments = uri.Segments;

            // Comprobar que la URL tiene suficientes segmentos para extraer el public_id
            if (segments.Length > 3)
            {
                // Combina la carpeta y el nombre del archivo para crear el public_id completo
                var publicId = string.Join("", segments.Skip(3)).Split('.')[0]; // Extraer el public_id sin la extensión
                return publicId;
            }

            throw new ArgumentException("La URL de la imagen no es válida para extraer el public_id.");
        }




    }//end class
}//end namespace