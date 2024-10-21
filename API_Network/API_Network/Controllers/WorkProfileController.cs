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
    public class WorkProfileController : Controller
    {
        private readonly DbContextNetwork _context;
        private readonly IAutorizacionServicesWorkProfile autorizacionServices;
        private readonly CloudinaryController _cloudinaryController;

        public WorkProfileController(DbContextNetwork wpContext, IAutorizacionServicesWorkProfile autorizacionServices, CloudinaryController cloudinaryController)
        {
            _context = wpContext;
            this.autorizacionServices = autorizacionServices;
            _cloudinaryController = cloudinaryController;

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
        public async Task<IActionResult> CrearCuentaAsync(WorkProfileImage workProfile)
        {
            //verifica si ya hay un WorkProfile con los mismos datos
            bool workProfileExist = _context.WorkProfiles.Any(wp => wp.Email == workProfile.Email);
            var imageUrl = "";
            var publicId = "";

            try
            {
                if (!workProfileExist)
                {
                    if (workProfile.ProfilePictureUrl != null)
                    {
                        var result = await _cloudinaryController.SaveImage(workProfile.ProfilePictureUrl, "workProfile");

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
                    else if (workProfile.ProfilePictureUrl == null)
                    {
                        imageUrl = "ND";
                        publicId = "ND";
                    }

                    var newWorkProfile = new WorkProfile{
                        Name = workProfile.Name,
                        Email = workProfile.Email,
                        Number = workProfile.Number,
                        Password = workProfile.Password,
                        Province = workProfile.Province,
                        City = workProfile.City,
                        WorkDescription = workProfile.WorkDescription,
                        ProfilePictureUrl = imageUrl,
                        ProfileType= workProfile.ProfileType,
                        ImagePublicId =  publicId

                    };

                    //Proceso de encriptación
                    newWorkProfile.Salt = HelperCryptography.GenerateSalt();
                    byte[] hashedPassword = HelperCryptography.EncriptarPassword(workProfile.Password, newWorkProfile.Salt);
                    newWorkProfile.Password = Convert.ToBase64String(hashedPassword);

                    _context.WorkProfiles.Add(newWorkProfile);
                    _context.SaveChanges();
                    return Ok(new
                    {
                        message = "Cuenta Creada",
                        workId = newWorkProfile.WorkId,
                        email = newWorkProfile.Email,
                        password = newWorkProfile.Password
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
        }//end Consultar

        //[Authorize]
        [HttpPut("Editar")]
        public async Task<string> EditarAsync(WorkProfileImage workProfile)
        {
            string msj = "Error al editar el perfil";
            var workerExist = _context.WorkProfiles.FirstOrDefault(w => w.WorkId == workProfile.WorkId);

            try
            {
                if (workProfile.ProfilePictureUrl != null && workProfile.ProfilePictureUrl.Length > 0)
                {
                    var tempPublicId = workerExist.ImagePublicId;

                    var result = await _cloudinaryController.SaveImage(workProfile.ProfilePictureUrl, "workProfile");

                    if (result is OkObjectResult okResult)
                    {
                        var uploadResult = okResult.Value as dynamic;
                        if (uploadResult != null)
                        {
                            await _cloudinaryController.DeleteImage(tempPublicId);
                            workerExist.ProfilePictureUrl = uploadResult.Url;
                            workerExist.ImagePublicId = uploadResult.PublicId;
                        }
                    }
                }
                else if (workProfile.ProfilePictureUrl == null)
                {
                    workerExist.ProfilePictureUrl = "ND";
                    workerExist.ImagePublicId = "ND";
                }

                //se verifica si la contraseña ha sido cambiada
                if (!string.IsNullOrEmpty(workProfile.Password) && workProfile.Password != workerExist.Password)
                {
                    // Si la contraseña ha sido modificada, se encripta
                    byte[] hashedPassword = HelperCryptography.EncriptarPassword(workProfile.Password, workerExist.Salt);
                    workerExist.Password = Convert.ToBase64String(hashedPassword);
                }

                // Actualizar los demás campos del perfil con los datos recibidos de WorkProfileImage
                workerExist.Name = workProfile.Name;
                workerExist.Email = workProfile.Email;
                workerExist.Province = workProfile.Province;
                workerExist.City = workProfile.City;
                workerExist.WorkDescription = workProfile.WorkDescription;
                workerExist.ProfileType = workProfile.ProfileType;

                _context.WorkProfiles.Update(workerExist);
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
                        if (ws.WorkId == data.WorkId)
                        {
                            _context.WorkSkills.Remove(ws);
                            _context.SaveChanges();
                        }
                    }//end foreach
                    
                    //se elimina la imagen de cloudinary
                    await _cloudinaryController.DeleteImage(data.ImagePublicId);

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
                    //Proceso de encriptación
                    byte[] hashedPassword = HelperCryptography.EncriptarPassword(confirmarPassword, workProfile.Salt);
                    workProfile.Password = Convert.ToBase64String(hashedPassword);

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

        //Modificado para que sea por medio de un objeto y no con parametros escritos, asi mas facil y seguro de implementar en el front/
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            //se busca al usuario por el email
            var worker = await _context.WorkProfiles.FirstOrDefaultAsync(w => w.Email.Equals(loginRequest.Email));

            if (worker == null)
            {
                //si el usuario no existe
                return Unauthorized();
            }
            else
            {
                //se encriptar la contraseña con el mismo "salt" que está en la base de datos
                byte[] hashedPassword = HelperCryptography.EncriptarPassword(loginRequest.Password, worker.Salt);

                //se convierte la contraseña almacenada (que está en Base64) a byte[] para compararla
                byte[] storedPassword = Convert.FromBase64String(worker.Password);

                //se comparan las contraseñas encriptadas
                if (!HelperCryptography.CompareArrays(hashedPassword, storedPassword))
                {
                    return Unauthorized();
                }

                //si la contraseña es correcta, generar el token
                var autorizado = await autorizacionServices.DevolverToken(worker);

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
