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

        ////[Authorize]
        [HttpGet("ListadoGeneral")]
        public async Task<List<Post>> Listado()
        {
            var list = await _context.Posts
                .Include(p => p.WorkProfile)
                .Select(p => new Post
                {
                    PostId = p.PostId,
                    WorkId = p.WorkId,
                    ContentPost = p.ContentPost,
                    PaymentReceipt = p.PaymentReceipt,
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
                        Password = "ND",
                        Province = p.WorkProfile.Province,
                        City = p.WorkProfile.City,
                        WorkDescription = p.WorkProfile.WorkDescription,
                        ProfilePictureUrl = p.WorkProfile.ProfilePictureUrl,
                        ProfileType = p.WorkProfile.ProfileType,
                        Salt = "ND"
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
            }
        }

        ////[Authorize]
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
            }
        }

        ////[Authorize]
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
            }
        }

        ////[Authorize]
        [HttpGet("ConsultarWorkId")]
        public async Task<ActionResult<List<Post>>> ConsultarWorkId(int workId)
        {
            var posts = await _context.Posts
                             .Where(ws => ws.WorkId == workId)
                             .Include(p => p.WorkProfile) // Incluye WorkProfile
                             .ToListAsync();
            return posts;
        }

        ////[Authorize]
        [HttpGet("Consultar")]
        public async Task<Post> Consultar(int postId)
        {
            var temp = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);

            return temp;
        }

        //[Authorize]
        [HttpPost("Agregar")]
        public async Task<string> Agregar(PostImage post)
        {
            string msj = "";
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == post.WorkId); //verifica que el workId exista
            var imageUrl = "";
            var publicId = "";

            try
            {
                if (post.PaymentReceipt <= 0)
                {
                    return "No se puede registrar el post sin un recibo de pago v�lido.";
                }
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
                        ImagePublicId = publicId,
                        PaymentReceipt = post.PaymentReceipt
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
            }
            return msj;
        }
        
        ////[Authorize]
        [HttpPut("Editar")]
        public async Task<string> Editar(PostImage post)
        {
            string msj = "Error al editar el post";
            bool workExist = _context.WorkProfiles.Any(wp => wp.WorkId == post.WorkId); //verifica que el workId exista
            var postExist = _context.Posts.FirstOrDefault(p => p.PostId == post.PostId);

            try
            {
                if (post.PostImageUrl != null)
                {
                    var tempPublicId = postExist.ImagePublicId;
                    var result = await _cloudinaryController.SaveImage(post.PostImageUrl, "posts");
                    if (result is OkObjectResult okResult)
                    {
                        var uploadResult = okResult.Value as dynamic;
                        if (uploadResult != null)
                        {
                            await _cloudinaryController.DeleteImage(tempPublicId);
                            postExist.PostImageUrl = uploadResult.Url;
                            postExist.ImagePublicId = uploadResult.PublicId;
                        }
                    }
                }
                else
                {
                    postExist.PostImageUrl = postExist.PostImageUrl ?? "ND";
                    postExist.ImagePublicId = postExist.ImagePublicId ?? "ND";
                }
                postExist.WorkId = post.WorkId;
                postExist.ContentPost = post.ContentPost;
                postExist.CreateAt = post.CreateAt;
                //postExist.LikesCount = post.LikesCount; //se comenta para que no se reseteen cuando se edita
                //postExist.CommentsCount = post.CommentsCount; //se comenta para que no se reseteen cuando se edita
                postExist.Approved = post.Approved;
                postExist.PaymentReceipt = post.PaymentReceipt;
                if (workExist)
                {
                    _context.Posts.Update(postExist);
                    _context.SaveChanges();
                    msj = "Post editado correctamente";
                }
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }

            return msj;
        }

        ////[Authorize]
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(int postId)
        {
            string msj;

            try
            {
                var likes = await _context.Likes.ToListAsync(); //se eliminan todos los likes relacionados a este post
                foreach (var like in likes)
                {
                    if (like.PostId == postId)
                    {
                        _context.Likes.Remove(like);
                        _context.SaveChanges();
                    }
                }

                var comments = await _context.Comments.ToListAsync();
                foreach (var comment in comments)
                {
                    if (comment.PostId == postId)
                    { 
                        _context.Comments.Remove(comment);
                        _context.SaveChanges();
                    }
                }

                var postToDelete = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId); // Buscar el post en la base de datos
                await _cloudinaryController.DeleteImage(postToDelete.ImagePublicId); //se elimina la imagen de cloudinary
                _context.Posts.Remove(postToDelete); // Si la imagen fue eliminada correctamente, eliminar el post de la base de datos
                await _context.SaveChangesAsync();
                msj = $"El PostId: {postToDelete.PostId}, ha sido eliminado correcatamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException?.Message}";
            }

            return msj;
        }
    }
}