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
            // Comprobar si ambos son nulos
            if (userId == null && workProfileId == null)
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

            // Se busca si hay un "like" que coincida exactamente con PostId y con el UserId
            if (likePost.UserId != null)
            {
                likeExist = await (from like in _context.Likes
                                   where like.PostId == likePost.PostId &&
                                           (like.UserId == likePost.UserId)
                                   select like).FirstOrDefaultAsync();
            }

            // Se busca si hay un "like" que coincida exactamente con PostId y con el WorkProfileId
            if (likePost.WorkProfileId != null)
            {
                likeExist = await (from like in _context.Likes
                                   where like.PostId == likePost.PostId &&
                                           (like.WorkProfileId == likePost.WorkProfileId)
                                   select like).FirstOrDefaultAsync();
            }

            try
            {
                //si es el User el que da el like
                if (likePost.UserId != null && likeExist == null)
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
        }//end LikePost

        //[Authorize]
        [HttpPost("LikeComment")]
        public async Task<string> LikeComment(LikeComment likeComment)
        {
            var msj = "";
            var likeExist = new Like();

            //se cambian los valores que esten en 0 a null para que no hayan errores
            if (likeComment.UserId == 0)
            {
                likeComment.UserId = null;
            }
            if (likeComment.WorkProfileId == 0)
            {
                likeComment.WorkProfileId = null;
            }

            // Se busca el "like" que coincida exactamente con CommentId y con el UserId
            if (likeComment.UserId != null)
            {
                likeExist = await (from like in _context.Likes
                                   where like.CommentId == likeComment.CommentId &&
                                           (like.UserId == likeComment.UserId)
                                   select like).FirstOrDefaultAsync();
            }

            // Se busca el "like" que coincida exactamente con PostId y con el WorkProfileId
            if (likeComment.WorkProfileId != null)
            {
                likeExist = await (from like in _context.Likes
                                   where like.CommentId == likeComment.CommentId &&
                                           (like.WorkProfileId == likeComment.WorkProfileId)
                                   select like).FirstOrDefaultAsync();
            }

            try
            {
                //si es el User el que da el like
                if (likeComment.UserId != null && likeExist == null)
                {
                    var like = new Like
                    {
                        PostId = null,
                        CommentId = likeComment.CommentId,
                        UserId = likeComment.UserId,
                        WorkProfileId = null
                    };

                    //Actualizar el campo LikesCount en Comment
                    var comments = await _context.Comments.ToListAsync();
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

                    //Actualizar el campo LikesCount en Comment
                    var comments = await _context.Comments.ToListAsync();
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
        }//end LikePost

        //[Authorize]
        [HttpDelete("DislikePost")]
        public async Task<IActionResult> DislikePost(LikePost likePost)
        {
            string msj = "";
            var likeToDelete = new Like();

            // Se cambian los valores que estén en 0 a null para evitar errores
            if (likePost.UserId == 0)
            {
                likePost.UserId = null;
            }
            if (likePost.WorkProfileId == 0)
            {
                likePost.WorkProfileId = null;
            }

            // Se busca el "like" que coincida exactamente con PostId y con el UserId
            if (likePost.UserId != null)
            {
                likeToDelete = await (from like in _context.Likes
                                      where like.PostId == likePost.PostId &&
                                              (like.UserId == likePost.UserId)
                                      select like).FirstOrDefaultAsync();
            }

            // Se busca el "like" que coincida exactamente con PostId y con el WorkProfileId
            if (likePost.WorkProfileId != null)
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
                    //Actualizar el campo LikesCount en Post
                    var posts = await _context.Posts.ToListAsync();
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
        }//end DislikePost

        //[Authorize]
        [HttpDelete("DislikeComment")]
        public async Task<IActionResult> DislikeComment(LikeComment likeComment)
        {
            string msj = "";

            var likeToDelete = new Like();

            // Se cambian los valores que estén en 0 a null para evitar errores
            if (likeComment.UserId == 0)
            {
                likeComment.UserId = null;
            }
            if (likeComment.WorkProfileId == 0)
            {
                likeComment.WorkProfileId = null;

            }

            // Se busca el "like" que coincida exactamente con CommentId y con el UserId
            if (likeComment.UserId != null)
            {
                likeToDelete = await (from like in _context.Likes
                                      where like.CommentId == likeComment.CommentId &&
                                              (like.UserId == likeComment.UserId)
                                      select like).FirstOrDefaultAsync();
            }

            // Se busca el "like" que coincida exactamente con CommentId y con el WorkProfileId
            if (likeComment.WorkProfileId != null)
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
                     //Actualizar el campo LikesCount en Comment
                    var comments = await _context.Comments.ToListAsync();
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


    }//end class
}//end namespace