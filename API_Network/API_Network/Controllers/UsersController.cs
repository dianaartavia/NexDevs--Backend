using API_Network.Context;
using API_Network.Models;
using API_Network.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using API_Network.Helpers;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

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
        public string CrearCuenta(User user)
        {
            string msj = "";

            //verifica si ya hay un User con los mismos datos
            bool userExist = _context.Users.Any(u => u.Email == user.Email);

            try
            {
                if (!userExist)
                {
                    if (user.ProfileType == null)
                    {
                        user.ProfileType = 'U';
                    }
                    user.Salt = HelperCryptography.GenerateSalt();
                    _context.Users.Add(user);
                    _context.SaveChanges();
                    msj = "Cuenta Creada";
                }
                else
                {
                    msj = "Ya existe una cuenta con ese correo o los datos son incorrectos";
                }
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end

            return msj;
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
        public async Task<IActionResult> Login(string email, string password)
        {
            var temp = await _context.Users.FirstOrDefaultAsync(u => (u.Email.Equals(email)) && (u.Password.Equals(password)));

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
