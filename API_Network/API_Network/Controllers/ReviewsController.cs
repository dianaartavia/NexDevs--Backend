using API_Network.Context;
using API_Network.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Network.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class ReviewsController : Controller
    {
        private readonly DbContextNetwork _context;

        public ReviewsController(DbContextNetwork context)
        {
            _context = context;
        }

        //[Authorize]
        [HttpGet("Listado")]
        public async Task<List<Review>> Listado()
        {
            var list = await _context.Reviews.ToListAsync();

            if (list == null)
            {
                return new List<Review>();
            }
            else
            {
                return list;
            }
        }

        //[Authorize]
        [HttpGet("Consultar")]
        public async Task<Review> ConsultarId(int reviewId)
        {
            var temp = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);

            return temp;
        }

        //[Authorize]
        [HttpGet("ConsultarWorkId")]
        public async Task<List<ReviewUserData>> ConsultarWorkId(int workId)
        {
            var reviews = await (from review in _context.Reviews
                                 join user in _context.Users on review.UserId equals user.UserId
                                 where review.WorkId == workId
                                 select new ReviewUserData
                                 {
                                     ReviewId = review.ReviewId,
                                     WorkId = review.WorkId,
                                     ReviewComment = review.ReviewComment,
                                     CreateAt = review.CreateAt,
                                     UserId = user.UserId,
                                     FirstName = user.FirstName,
                                     LastName = user.LastName,
                                     ProfilePictureUrlUser = user.ProfilePictureUrl,
                                     ImagePublicId = user.ImagePublicId
                                 }).ToListAsync();
            return reviews; // Retornar la lista con los resultados
        }

        //[Authorize]
        [HttpPost("Agregar")]
        public string Agregar(Review review)
        {
            string msj = "";
            try
            {
                _context.Reviews.Add(review);
                _context.SaveChanges();
                msj = "Review registrada correctamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }

        //[Authorize]
        [HttpPut("Editar")]
        public string Editar(Review review)
        {
            string msj = "";
            try
            {
                _context.Reviews.Update(review);
                _context.SaveChanges();
                msj = "Review editada correctamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }

        //[Authorize]
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(int reviewId)
        {
            string msj = "";
            try
            {
                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
                if (review != null)
                {
                    _context.Reviews.Remove(review);
                    _context.SaveChanges();
                    msj = $"Review eliminada correctamente";
                }
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }
    }
}
