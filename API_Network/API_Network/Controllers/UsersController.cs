using API_Network.Context;
using API_Network.Models;
using API_Network.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using API_Network.Helpers;
using Microsoft.AspNetCore.Identity.Data;


namespace API_Network.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly DbContextNetwork _context;
        private readonly IAutorizacionServicesUser autorizacionServices;

        private readonly CloudinaryController _cloudinaryController;

        public UsersController(DbContextNetwork uContext, IAutorizacionServicesUser autorizacionServices, CloudinaryController cloudinaryController)
        {
            _context = uContext;
            this.autorizacionServices = autorizacionServices;
            _cloudinaryController = cloudinaryController;
        }//end UsersController

        // [Authorize]
        [HttpGet("Listado")]
        public async Task<List<User>> Listado()
        {
            var list = await _context.Users.ToListAsync();
            if (list == null)
            {
                return new List<User>();
            }
            else
            {
                return list;
            }//end else
        }//end Listado

        [HttpPost("CrearCuenta")]
        public async Task<IActionResult> CrearCuenta(UserImage user)
        {
            //verifica si ya hay un User con los mismos datos
            bool userExist = _context.Users.Any(u => u.Email == user.Email);
            var imageUrl = "";
            var publicId = "";

            try
            {
                if (!userExist)
                {
                    if (user.ProfilePictureUrl != null)
                    {
                        // Llamar al método de subida de imagen
                        var result = await _cloudinaryController.SaveImage(user.ProfilePictureUrl);

                        if (result is OkObjectResult okResult)
                        {
                            // Extraer los valores de la respuesta
                            var uploadResult = okResult.Value as dynamic;

                            if (uploadResult != null)
                            {
                                publicId = uploadResult.PublicId;
                                imageUrl = uploadResult.Url;
                            }
                        }
                    }
                    else if (user.ProfilePictureUrl == null)
                    {
                        imageUrl = "ND";
                        publicId = "ND";
                    }

                    var newUser = new User
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Password = user.Password,
                        Province = user.Province,
                        City = user.City,
                        Bio = user.Bio,
                        ProfilePictureUrl = imageUrl,
                        ProfileType = 'U',
                        ImagePublicId = publicId

                    };

                    newUser.Salt = HelperCryptography.GenerateSalt();
                    _context.Users.Add(newUser);
                    _context.SaveChanges();
                    return Ok(new
                    {
                        message = "Cuenta Creada",
                        userId = newUser.UserId,
                        email = newUser.Email,
                        password = newUser.Password
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        message = "Ya existe una cuenta con ese correo o los datos son incorrectos"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = $"Error: {ex.Message}",
                    innerException = ex.InnerException?.Message
                });
            }//end
        }//end CrearCuenta

        //[Authorize]
        [HttpGet("BuscarEmail")]
        public async Task<User> Consultar(string email)
        {
            var temp = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return temp;
        }//end Consultar

        //[Authorize]
        [HttpPut("Editar")]
        public string Editar(User user)
        {
            string msj = "Error al editar el perfil";
            try
            {
                _context.Users.Update(user);
                _context.SaveChanges();
                msj = "Perfil editado correctamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end
            return msj;
        }//end Editar

        //[Authorize]
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(string email)
        {
            string msj = "No se ha podido eliminar el perfil";

            try
            {
                var data = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (data != null)
                {
                    _context.Users.Remove(data);
                    _context.SaveChanges();
                    msj = $"El perfil de {data.FirstName} {data.LastName}, ha sido eliminado correctamente";
                }
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end
            return msj;
        }//end Eliminar

        // ***   MÉTODOS PARA RECUPERAR CONTRASEÑA    ***

        [HttpPost("Restablecer")]
        public async Task<string> RestablecerAsync(string email)
        {
            string msj = "";

            //se verifica si existe un usuario con ese email
            bool emailExist = _context.Users.Any(u => u.Email.Equals(email));

            if (emailExist)
            {
                User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                //enviar email
                await EnviarEmail(email, user.UserId.ToString());
                msj = "Correo enviado";
            }
            else
            {
                msj = "Este correo no se encuentra registrado";
            }//end else

            return msj;
        }//end Restablecer

        [HttpPost("NuevaContraseña")]
        public async Task<string> NuevaContraseña(string userId, string password, string confirmarPassword)
        {
            string msj = "";

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));
                if (password.Equals(confirmarPassword))
                {
                    user.Password = password;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    return msj = "Se ha restablecido la contraseña exitosamente";
                }//end if
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end catch

            return msj;
        }//end Restablecer

        private async Task<bool> EnviarEmail(String email, string userId)
        {
            string mensaje = "";
            bool enviado = false;
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            try
            {
                EmailRestablecer emailRestablecer = new EmailRestablecer();
                emailRestablecer.Enviar(email, userId);
                enviado = true;

                return enviado;
            }
            catch (Exception e)
            {
                mensaje = "Error al enviar el correo " + e.Message;

                return false;
            }
        }//end EnviarEmail

        // ***   MÉTODO AUTENTICACION DE LOGIN    ***

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var temp = await _context.Users.FirstOrDefaultAsync(u => (u.Email.Equals(loginRequest.Email)) && (u.Password.Equals(loginRequest.Password)));

            if (temp == null)
            {
                return Unauthorized();
            }
            else
            {
                var autorizado = await autorizacionServices.DevolverToken(temp);

                if (autorizado == null)
                {
                    return Unauthorized();
                }
                else
                {
                    return Ok(autorizado);
                }//end else
            }//end else
        }//end Autenticar
    }
}
