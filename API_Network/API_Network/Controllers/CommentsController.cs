using API_Network.Context;
using API_Network.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace API_Network.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class CommentsController : Controller
    {
        private readonly DbContextNetwork _context;

        public CommentsController(DbContextNetwork context)
        {
            _context = context;
        }

        //[Authorize]
        [HttpGet("Listado")]
        public async Task<List<Comment>> Listado()
        {
            var list = await _context.Comments.ToListAsync();

            if (list == null)
            {
                return new List<Comment>();
            }
            else
            {
                return list;
            }//end if/else
        }//end Listado

        //[Authorize]
        [HttpGet("ConsultarId")]
        public async Task<Comment> Consultar(int commentId)
        {
            var temp = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);

            return temp;
        }

        //[Authorize]
        [HttpGet("ConsultarPorPost")]
        public async Task<List<CommentData>> ConsultarPost(int postId)
        {
            var commentDataList = new List<CommentData>();

            // Obtener comentarios con información de usuario
            var userComments = await (from comment in _context.Comments
                                      join user in _context.Users on comment.UserId equals user.UserId
                                      where comment.PostId == postId // Filtrar por postId
                                      select new CommentData
                                      {
                                          CommentId = comment.CommentId,
                                          PostId = comment.PostId,
                                          ContentComment = comment.ContentComment,
                                          CreateAt = comment.CreateAt,
                                          LikesCount = comment.LikesCount,
                                          WorkId = null, // No hay workId en este caso
                                          Name = null, // No hay nombre de trabajador
                                          ProfilePictureUrlWorker = null, // No hay foto de perfil de trabajador
                                          UserId = user.UserId,
                                          FirstName = user.FirstName,
                                          LastName = user.LastName,
                                          ProfilePictureUrlUser = user.ProfilePictureUrl
                                      }).ToListAsync();

            // Obtener comentarios con información de perfil de trabajo
            var workComments = await (from comment in _context.Comments
                                      join worker in _context.WorkProfiles on comment.WorkId equals worker.WorkId
                                      where comment.PostId == postId // Filtrar por postId
                                      select new CommentData
                                      {
                                          CommentId = comment.CommentId,
                                          PostId = comment.PostId,
                                          ContentComment = comment.ContentComment,
                                          CreateAt = comment.CreateAt,
                                          LikesCount = comment.LikesCount,
                                          WorkId = worker.WorkId,
                                          Name = worker.Name,
                                          ProfilePictureUrlWorker = worker.ProfilePictureUrl,
                                          UserId = null, // No hay UserId en este caso
                                          FirstName = null, // No hay nombre de usuario
                                          LastName = null, // No hay apellido de usuario
                                          ProfilePictureUrlUser = null // No hay foto de perfil de usuario
                                      }).ToListAsync();

            // Combinar ambas listas de comentarios
            commentDataList.AddRange(userComments);
            commentDataList.AddRange(workComments);

            return commentDataList;
        }

        //Authorize]
        [HttpPost("Like")]
        public async Task<string> Like(int commentId)
        {
            var msj = "";
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);

            comment.LikesCount += 1;

            try
            {
                _context.Comments.Update(comment);
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
        public async Task<string> Dislike(int commentId)
        {
            var msj = "";
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);

            comment.LikesCount = comment.LikesCount - 1;

            try
            {
                _context.Comments.Update(comment);
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
        public async Task<string> AgregarAsync(Comment comment)
        {
            string msj = "";

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == comment.PostId);

            post.CommentsCount += 1;

            if (comment.UserId == 0)
            {
                comment.UserId = null;
            }
            if (comment.WorkId == 0)
            {
                comment.WorkId = null;
            }

            try
            {
                _context.Posts.Update(post);
                _context.Comments.Add(comment);
                _context.SaveChanges();
                msj = "Comentario registrado correctamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end

            return msj;
        }//end Agregar

        //[Authorize]
        [HttpPut("Editar")]
        public string Editar(Comment comment)
        {
            string msj = "";
            try
            {

                _context.Comments.Update(comment);
                _context.SaveChanges();
                msj = "Comentario editado correctamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end

            return msj;
        }//end Editar

        //[Authorize]
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(int commentId)
        {
            string msj = "";

            try
            {
                var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);

                if (comment != null)
                {
                    //se resta el CommentsCount en posts
                    var posts = await _context.Posts.ToListAsync();

                    foreach (var post in posts)
                    {
                        if (post.PostId == comment.PostId)
                        {
                            post.CommentsCount = post.CommentsCount - 1;
                            _context.Posts.Update(post);
                        }
                    }//end foreach

                    _context.Comments.Remove(comment);
                    _context.SaveChanges();

                    msj = $"El comentario: {comment.CommentId}, ha sido eliminado correctamente";
                }
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }//end Eliminar
    }
}

