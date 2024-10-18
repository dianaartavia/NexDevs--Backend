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

       //[Authorize]
        [HttpPost("Agregar")]
        public string Agregar(Post post)
        {
            string msj = "";

            //verifica que el workId exista
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == post.WorkId);

            try
            {
                if (workExist)
                {
                    post.Approved = 0;
                    _context.Posts.Add(post);
                    _context.SaveChanges();
                    msj = "Post registrado correctamente";
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
                //se eliminan todos los likes relacionados a este post
                    var likes = await _context.Likes.ToListAsync();

                    foreach (var like in likes)
                    {
                        if (like.PostId == postId)
                        {
                            _context.Likes.Remove(like);
                            _context.SaveChanges();
                        }
                    }

                // Buscar el post en la base de datos
                var postToDelete = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);

                if (postToDelete == null)
                {
                    return $"No existe ning�n post con el ID: {postId}";
                }

                // Extraer el public_id de la URL de la imagen
                var publicId = GetPublicIdFromUrl(postToDelete.PostImageUrl);


                // Eliminar la imagen de Cloudinary
                var deleteParams = new DeletionParams(publicId);
                var deleteResult = await _cloudinary.DestroyAsync(deleteParams);

                // Verificar si la eliminaci�n fue exitosa
                if (deleteResult.StatusCode == HttpStatusCode.OK)
                {
                    // Si la imagen fue eliminada correctamente, eliminar el post de la base de datos
                    _context.Posts.Remove(postToDelete);
                    await _context.SaveChangesAsync();
                    msj = $"{publicId} Post con el ID {postToDelete.PostId} eliminado correctamente.";
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
                var publicId = string.Join("", segments.Skip(3)).Split('.')[0]; // Extraer el public_id sin la extensi�n
                return publicId;
            }

            throw new ArgumentException("La URL de la imagen no es v�lida para extraer el public_id.");
        }




    }//end class
}//end namespace