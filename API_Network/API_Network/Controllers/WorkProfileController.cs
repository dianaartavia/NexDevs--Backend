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
    public class WorkProfileController : Controller
    {
        private readonly DbContextNetwork _context;
        private readonly IAutorizacionServicesWorkProfile autorizacionServices;

        public WorkProfileController(DbContextNetwork wpContext, IAutorizacionServicesWorkProfile autorizacionServices)
        {
            _context = wpContext;
            this.autorizacionServices = autorizacionServices;
        }//end WorkProfilecontroller

        // [Authorize]
        [HttpGet("Listado")]
        public async Task<List<WorkProfile>> Listado()
        {
            var list = await _context.WorkProfiles.ToListAsync();
            if (list == null)
            {
                return new List<WorkProfile>();
            }
            else
            {
                return list;
            }//end else
        }//end Listado

        [HttpPost("CrearCuenta")]
        public string CrearCuenta(WorkProfile workProfile)
        {
            string msj = "";

            //verifica si ya hay un WorkProfile con los mismos datos
            bool workProfileExist = _context.WorkProfiles.Any(wp => wp.Email == workProfile.Email);

            try
            {
                if (!workProfileExist)
                {
                    workProfile.Salt = HelperCryptography.GenerateSalt();
                    _context.WorkProfiles.Add(workProfile);
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
        public async Task<WorkProfile> Consultar(string email)
        {
            var temp = await _context.WorkProfiles.FirstOrDefaultAsync(wp => wp.Email == email);
            return temp;
        }//end Consultar

        //[Authorize]
        [HttpGet("BuscarID")]
        public async Task<ActionResult<WorkProfile>> ConsultarID(int id)
        {
            var temp = await _context.WorkProfiles.FirstOrDefaultAsync(wp => wp.WorkId == id);
            return temp;
            // var workProfile = await _context.WorkProfiles
            //        .Where(wp => wp.WorkId == id)
            //        .Select(wp => new WorkProfile
            //        {
            //            WorkId = wp.WorkId,
            //            Name = wp.Name,
            //            Email = wp.Email,       
            //            Number = wp.Number,
            //            Province = wp.Province,
            //            City = wp.City,
            //            WorkDescription = wp.WorkDescription,
            //            ProfilePictureUrl = wp.ProfilePictureUrl,
            //            CategoryId = wp.CategoryId,
            //        })
            //        .FirstOrDefaultAsync();

            // if (workProfile == null)
            // {
            //     return NotFound();
            // }

            // return Ok(workProfile);
        }//end Consultar

        //[Authorize]
        [HttpPut("Editar")]
        public string Editar(WorkProfile workProfile)
        {
            string msj = "Error al editar el perfil";
            try
            {
                _context.WorkProfiles.Update(workProfile);
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
                var data = await _context.WorkProfiles.FirstOrDefaultAsync(wp => wp.Email == email);

                if (data != null)
                {
                    var listWorkSkills = await _context.WorkSkills.ToListAsync();

                    //se busca en la tabla workSkill todos los datos relacionados al workprofile y se eliminan
                    foreach (var ws in listWorkSkills)
                    {
                        if(ws.WorkId == data.WorkId)
                        {
                            _context.WorkSkills.Remove(ws);
                            _context.SaveChanges();
                        }
                    }//end foreach

                    _context.WorkProfiles.Remove(data);
                    _context.SaveChanges();
                    msj = $"El perfil de {data.Name}, ha sido eliminado correctamente";
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
            bool emailExist = _context.WorkProfiles.Any(wp => wp.Email.Equals(email));

            if (emailExist)
            {
                WorkProfile workProfile = await _context.WorkProfiles.FirstOrDefaultAsync(wp => wp.Email == email);

                //enviar email
                await EnviarEmail(email, workProfile.WorkId.ToString());
                msj = "Correo enviado";
            }
            else
            {
                msj = "Este correo no se encuentra registrado";
            }//end else

            return msj;
        }//end Restablecer

        [HttpPost("NuevaContraseña")]
        public async Task<string> NuevaContraseña(string workId, string password, string confirmarPassword)
        {
            string msj = "";

            try
            {
                var workProfile = await _context.WorkProfiles.FirstOrDefaultAsync(wp => wp.WorkId == int.Parse(workId));
                if (password.Equals(confirmarPassword))
                {
                    workProfile.Password = password;
                    _context.WorkProfiles.Update(workProfile);
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

        private async Task<bool> EnviarEmail(String email, string workId)
        {
            string mensaje = "";
            bool enviado = false;
            WorkProfile workProfile = await _context.WorkProfiles.FirstOrDefaultAsync(wp => wp.Email == email);

            try
            {
                EmailRestablecer emailRestablecer = new EmailRestablecer();
                emailRestablecer.Enviar(email, workId);
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
            var temp = await _context.WorkProfiles.FirstOrDefaultAsync(wp => (wp.Email.Equals(email)) && (wp.Password.Equals(password)));

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
