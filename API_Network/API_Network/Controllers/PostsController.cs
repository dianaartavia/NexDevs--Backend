using API_Network.Context;
using API_Network.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Network.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class PostsController : Controller
    {
        private readonly DbContextNetwork _context;

        public PostsController(DbContextNetwork pContext)
        {
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
            string msj = "";

            try
            {
                var temp = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
                if (temp == null)
                {
                    msj = $"No existe ningun Post con el id: {postId}";
                }
                else
                {
                    _context.Posts.Remove(temp);
                    await _context.SaveChangesAsync();
                    msj = $"Post con el ID {temp.PostId}, eliminado correctamente";
                }//end else
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }//end Eliminar

    }//end class
}//end namespace