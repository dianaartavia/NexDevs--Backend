using System.Runtime.InteropServices;
using API_Network.Context;
using API_Network.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Network.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class LikesController : Controller
    {
        //Authorize]
        private readonly DbContextNetwork _context;

        public LikesController(DbContextNetwork context)
        {
            _context = context;
        }

        [HttpGet("CheckIfIsLiked")]
        public async Task<bool> CheckIfIsLiked(int postId, int? userId = null, int? workProfileId = null)
        {
            if (userId == null && workProfileId == null) // Comprobar si ambos son nulos
            {
                return false;
            }
            // Verificar si existe el "like"
            var likeExists = await _context.Likes.AnyAsync(like => 
                like.PostId == postId &&
                (like.UserId == userId || like.WorkProfileId == workProfileId));

            return likeExists;
        } 

        //[Authorize]
        [HttpPost("LikePost")]
        public async Task<string> LikePost(LikePost likePost)
        {
            var msj = "";
            var likeExist = new Like();
            //se cambian los valores que esten en 0 a null para que no hayan errores
            if (likePost.UserId == 0)
            {
                likePost.UserId = null;
            }
            if (likePost.WorkProfileId == 0)
            {
                likePost.WorkProfileId = null;
            }
            if (likePost.UserId != null) // Se busca si hay un "like" que coincida exactamente con PostId y con el UserId
            {
                likeExist = await (from like in _context.Likes
                                   where like.PostId == likePost.PostId &&
                                           (like.UserId == likePost.UserId)
                                   select like).FirstOrDefaultAsync();
            }
            if (likePost.WorkProfileId != null) // Se busca si hay un "like" que coincida exactamente con PostId y con el WorkProfileId
            {
                likeExist = await (from like in _context.Likes
                                   where like.PostId == likePost.PostId &&
                                           (like.WorkProfileId == likePost.WorkProfileId)
                                   select like).FirstOrDefaultAsync();
            }

            try
            {
                if (likePost.UserId != null && likeExist == null) //si es el User el que da el like
                {
                    var like = new Like
                    {
                        PostId = likePost.PostId,
                        CommentId = null,
                        UserId = likePost.UserId,
                        WorkProfileId = null
                    };

                    //Actualizar el campo LikesCount en Post
                    var posts = await _context.Posts.ToListAsync();
                    var updatePost = new Post();

                    foreach (var post in posts)
                    {
                        if (post.PostId == likePost.PostId)
                        {
                            updatePost = post;
                            updatePost.LikesCount += 1;
                            _context.Posts.Update(updatePost);
                            _context.SaveChanges();
                            break;
                        }
                    }

                    _context.Likes.Add(like);
                    _context.SaveChanges();

                    msj = $"Like en el PostId: {likePost.PostId} registrado correctamente";
                }
                else if (likePost.WorkProfileId != null && likeExist == null) //si es el WorkProfile el que da el like
                {
                    var like = new Like
                    {
                        PostId = likePost.PostId,
                        CommentId = null,
                        UserId = null,
                        WorkProfileId = likePost.WorkProfileId
                    };
                    var posts = await _context.Posts.ToListAsync(); //Actualizar el campo LikesCount en Post
                    var updatePost = new Post();

                    foreach (var post in posts)
                    {
                        if (post.PostId == likePost.PostId)
                        {
                            updatePost = post;
                            updatePost.LikesCount += 1;
                            _context.Posts.Update(updatePost);
                            _context.SaveChanges();
                            break;
                        }
                    }
                    _context.Likes.Add(like);
                    _context.SaveChanges();
                    msj = $"Like en el PostId: {likePost.PostId} registrado correctamente";
                }
                else
                {
                    msj = $"Ya existe un like con los mismos datos registrado";
                }

            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }

        //[Authorize]
        [HttpPost("LikeComment")]
        public async Task<string> LikeComment(LikeComment likeComment)
        {
            var msj = "";
            var likeExist = new Like();
            if (likeComment.UserId == 0) //se cambian los valores que esten en 0 a null para que no hayan errores
            {
                likeComment.UserId = null;
            }
            if (likeComment.WorkProfileId == 0)
            {
                likeComment.WorkProfileId = null;
            }
            if (likeComment.UserId != null) // Se busca el "like" que coincida exactamente con CommentId y con el UserId
            {
                likeExist = await (from like in _context.Likes
                                   where like.CommentId == likeComment.CommentId &&
                                           (like.UserId == likeComment.UserId)
                                   select like).FirstOrDefaultAsync();
            }
            if (likeComment.WorkProfileId != null) // Se busca el "like" que coincida exactamente con PostId y con el WorkProfileId
            {
                likeExist = await (from like in _context.Likes
                                   where like.CommentId == likeComment.CommentId &&
                                           (like.WorkProfileId == likeComment.WorkProfileId)
                                   select like).FirstOrDefaultAsync();
            }
            try
            {
                if (likeComment.UserId != null && likeExist == null) //si es el User el que da el like
                {
                    var like = new Like
                    {
                        PostId = null,
                        CommentId = likeComment.CommentId,
                        UserId = likeComment.UserId,
                        WorkProfileId = null
                    };
                    var comments = await _context.Comments.ToListAsync(); //Actualizar el campo LikesCount en Comment
                    var updateComment = new Comment();
                    foreach (var comment in comments)
                    {
                        if (comment.CommentId == likeComment.CommentId)
                        {
                            updateComment = comment;
                            updateComment.LikesCount += 1;
                            _context.Comments.Update(updateComment);
                            _context.SaveChanges();
                            break;
                        }
                    }
                    _context.Likes.Add(like);
                    _context.SaveChanges();
                    msj = $"Like en el CommentId: {likeComment.CommentId} registrado correctamente";
                }
                else if (likeComment.WorkProfileId != null && likeExist == null) //si es el WorkProfile el que da el like
                {
                    var like = new Like
                    {
                        PostId = null,
                        CommentId = likeComment.CommentId,
                        UserId = null,
                        WorkProfileId = likeComment.WorkProfileId
                    };
                    var comments = await _context.Comments.ToListAsync(); //Actualizar el campo LikesCount en Comment
                    var updateComment = new Comment();
                    foreach (var comment in comments)
                    {
                        if (comment.CommentId == likeComment.CommentId)
                        {
                            updateComment = comment;
                            updateComment.LikesCount += 1;
                            _context.Comments.Update(updateComment);
                            _context.SaveChanges();
                            break;
                        }
                    }
                    _context.Likes.Add(like);
                    _context.SaveChanges();
                    msj = $"Like en el CommentId: {likeComment.CommentId} registrado correctamente";
                }
                else
                {
                    msj = $"Ya existe un like con los mismos datos registrado";
                }
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }

        //[Authorize]
        [HttpDelete("DislikePost")]
        public async Task<IActionResult> DislikePost(LikePost likePost)
        {
            string msj = "";
            var likeToDelete = new Like();
            if (likePost.UserId == 0) // Se cambian los valores que estén en 0 a null para evitar errores
            {
                likePost.UserId = null;
            }
            if (likePost.WorkProfileId == 0)
            {
                likePost.WorkProfileId = null;
            }
            if (likePost.UserId != null) // Se busca el "like" que coincida exactamente con PostId y con el UserId
            {
                likeToDelete = await (from like in _context.Likes
                                      where like.PostId == likePost.PostId &&
                                              (like.UserId == likePost.UserId)
                                      select like).FirstOrDefaultAsync();
            }
            if (likePost.WorkProfileId != null) // Se busca el "like" que coincida exactamente con PostId y con el WorkProfileId
            {
                likeToDelete = await (from like in _context.Likes
                                      where like.PostId == likePost.PostId &&
                                              (like.WorkProfileId == likePost.WorkProfileId)
                                      select like).FirstOrDefaultAsync();
            }
            try
            {
                if (likeToDelete != null)
                {
                    var posts = await _context.Posts.ToListAsync(); //Actualizar el campo LikesCount en Post
                    var updatePost = new Post();
                    foreach (var post in posts)
                    {
                        if (post.PostId == likePost.PostId)
                        {
                            updatePost = post;
                            updatePost.LikesCount--;
                            _context.Posts.Update(updatePost);
                            _context.SaveChanges();
                            break;
                        }
                    }
                    _context.Likes.Remove(likeToDelete);
                    await _context.SaveChangesAsync();
                    msj = $"El like en el CommentId: {likePost.PostId}, ha sido eliminado correctamente";
                }
                else
                {
                    return NotFound($"No se encontró un like para el CommentId: {likePost.PostId} con el UserId/WorkProfileId proporcionado.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message} {ex.InnerException?.ToString()}");
            }

            return Ok(msj);
        }

        //[Authorize]
        [HttpDelete("DislikeComment")]
        public async Task<IActionResult> DislikeComment(LikeComment likeComment)
        {
            string msj = "";

            var likeToDelete = new Like();
            if (likeComment.UserId == 0) // Se cambian los valores que estén en 0 a null para evitar errores
            {
                likeComment.UserId = null;
            }
            if (likeComment.WorkProfileId == 0)
            {
                likeComment.WorkProfileId = null;
            }
            if (likeComment.UserId != null) // Se busca el "like" que coincida exactamente con CommentId y con el UserId
            {
                likeToDelete = await (from like in _context.Likes
                                      where like.CommentId == likeComment.CommentId &&
                                              (like.UserId == likeComment.UserId)
                                      select like).FirstOrDefaultAsync();
            }
            if (likeComment.WorkProfileId != null) // Se busca el "like" que coincida exactamente con CommentId y con el WorkProfileId
            {
                likeToDelete = await (from like in _context.Likes
                                      where like.CommentId == likeComment.CommentId &&
                                              (like.WorkProfileId == likeComment.WorkProfileId)
                                      select like).FirstOrDefaultAsync();
            }
            try
            {
                if (likeToDelete != null)
                {
                    var comments = await _context.Comments.ToListAsync(); //Actualizar el campo LikesCount en Comment
                    var updateComment = new Comment();
                    foreach (var comment in comments)
                    {
                        if (comment.CommentId == likeComment.CommentId)
                        {
                            updateComment = comment;
                            updateComment.LikesCount--;
                            _context.Comments.Update(updateComment);
                            _context.SaveChanges();
                            break;
                        }
                    }

                    _context.Likes.Remove(likeToDelete);
                    await _context.SaveChangesAsync();
                    msj = $"El like en el CommentId: {likeComment.CommentId}, ha sido eliminado correctamente";
                }
                else
                {
                    return NotFound($"No se encontró un like para el CommentId: {likeComment.CommentId} con el UserId/WorkProfileId proporcionado.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message} {ex.InnerException?.ToString()}");
            }

            return Ok(msj);
        }
        [HttpGet("LikedItems")]
        public async Task<IActionResult> GetLikedItems(int? userId = null, int? workProfileId = null)
        {
            if (userId == null && workProfileId == null)
            {
                return BadRequest("Se debe proporcionar un UserId o WorkProfileId.");
            }

            var likedItems = await _context.Likes
                .Include(like => like.Post)
                .ThenInclude(post => post.WorkProfile)
                .Include(like => like.Comment)
                .Where(like => like.UserId == userId || like.WorkProfileId == workProfileId)
                .Select(like => new
                {
                        like.Post.PostId,
                        like.Post.WorkId,
                        like.Post.ContentPost,
                        like.Post.PaymentReceipt,
                        like.Post.PostImageUrl,
                        like.Post.CreateAt,
                        like.Post.LikesCount,
                        like.Post.CommentsCount,
                        like.Post.Approved,
                        WorkProfile = new
                        {
                            like.Post.WorkProfile.WorkId,
                            like.Post.WorkProfile.Name,
                            like.Post.WorkProfile.Email,
                            like.Post.WorkProfile.Number,
                            like.Post.WorkProfile.Province,
                            like.Post.WorkProfile.City,
                            like.Post.WorkProfile.WorkDescription,
                            like.Post.WorkProfile.ProfilePictureUrl,
                            like.Post.WorkProfile.ProfileType
                        }
   
                })
                .ToListAsync();

            return Ok(likedItems);
        }

    }
}