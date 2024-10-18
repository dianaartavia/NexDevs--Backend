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

        public UsersController(DbContextNetwork uContext, IAutorizacionServicesUser autorizacionServices)
        {
            _context = uContext;
            this.autorizacionServices = autorizacionServices;
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
        public IActionResult CrearCuenta(User user)
        {
            //verifica si ya hay un User con los mismos datos
            bool userExist = _context.Users.Any(u => u.Email == user.Email);

            try
            {
                if (!userExist)
                {
                    //Proceso de encriptación
                    user.Salt = HelperCryptography.GenerateSalt();
                    byte[] hashedPassword = HelperCryptography.EncriptarPassword(user.Password, user.Salt);
                    user.Password = Convert.ToBase64String(hashedPassword);

                    _context.Users.Add(user);
                    _context.SaveChanges();
                    return Ok(new
                    {
                        message = "Cuenta Creada",
                        userId = user.UserId,
                        email = user.Email,
                        password = user.Password
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
                //se busca el usuario actual en la base de datos
                var existingUser = _context.Users.FirstOrDefault(u => u.UserId == user.UserId);

                if (existingUser == null)
                {
                    return "Usuario no encontrado";
                }

                //se verifica si la contraseña ha sido cambiada
                if (!string.IsNullOrEmpty(user.Password) && user.Password != existingUser.Password)
                {
                    //si la contraseña ha sido modificada, se encripta
                    byte[] hashedPassword = HelperCryptography.EncriptarPassword(user.Password, existingUser.Salt);
                    existingUser.Password = Convert.ToBase64String(hashedPassword);
                }

                //se actualizan los otros campos del User (excepto la contraseña si no ha cambiado)
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.Email = user.Email;
                existingUser.Province = user.Province;
                existingUser.City = user.City;
                existingUser.Bio = user.Bio;
                existingUser.ProfilePictureUrl = user.ProfilePictureUrl;
                existingUser.ImagePublicId = user.ImagePublicId;

                _context.Users.Update(existingUser);
                _context.SaveChanges();

                msj = "Perfil editado correctamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException?.ToString()}";
            }

            return msj;
        }


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
        public async Task<string> Restablecer(string email)
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
                    //Proceso de encriptación
                    byte[] hashedPassword = HelperCryptography.EncriptarPassword(confirmarPassword, user.Salt);
                    user.Password = Convert.ToBase64String(hashedPassword);

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
            //se busca al usuario por el email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(loginRequest.Email));

            if (user == null)
            {
                //si el usuario no existe
                return Unauthorized();
            }
            else
            {
                //se encriptar la contraseña con el mismo "salt" que está en la base de datos
                byte[] hashedPassword = HelperCryptography.EncriptarPassword(loginRequest.Password, user.Salt);

                //se convierte la contraseña almacenada (que está en Base64) a byte[] para compararla
                byte[] storedPassword = Convert.FromBase64String(user.Password);

                //se comparan las contraseñas encriptadas
                if (!HelperCryptography.CompareArrays(hashedPassword, storedPassword))
                {
                    return Unauthorized();
                }

                //si la contraseña es correcta, generar el token
                var autorizado = await autorizacionServices.DevolverToken(user);

                if (autorizado == null)
                {
                    return Unauthorized();
                }
                else
                {
                    return Ok(autorizado);
                }
            }
        }//end Autenticar
    }
}
