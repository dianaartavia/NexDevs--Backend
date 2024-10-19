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
        private readonly CloudinaryController _cloudinaryController;

        public PostsController(DbContextNetwork pContext, CloudinaryController cloudinaryController)
        {
            _context = pContext;
            _cloudinaryController = cloudinaryController;
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
        public async Task<string> Agregar(PostImage post)
        {
            string msj = "";
            //verifica que el workId exista
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == post.WorkId);
            var imageUrl = "";
            var publicId = "";

            try
            {
                if (workExist)
                {
                    if (post.PostImageUrl != null)
                    {
                        var result = await _cloudinaryController.SaveImage(post.PostImageUrl, "posts");

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
                    else if (post.PostImageUrl == null)
                    {
                        imageUrl = "ND";
                        publicId = "ND";
                    }

                    var newPost = new Post
                    {
                        WorkId = post.WorkId,
                        ContentPost = post.ContentPost,
                        PostImageUrl = imageUrl,
                        CreateAt = post.CreateAt,
                        LikesCount = post.LikesCount,
                        CommentsCount = post.CommentsCount,
                        ImagePublicId = publicId
                    };

                    newPost.Approved = 0;
                    _context.Posts.Add(newPost);
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

                //se elimina la imagen de cloudinary
                await _cloudinaryController.DeleteImage(postToDelete.ImagePublicId);

                // Si la imagen fue eliminada correctamente, eliminar el post de la base de datos
                _context.Posts.Remove(postToDelete);
                await _context.SaveChangesAsync();
                msj =$"El PostId: {postToDelete.PostId}, ha sido eliminado correcatamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException?.Message}";
            }

            return msj;
        }
    }//end class
}//end namespace