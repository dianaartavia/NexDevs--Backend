using API_Network.Context;
using API_Network.Models;
using API_Network.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using API_Network.Helpers;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace API_Network.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkProfileController : Controller
    {
        private readonly DbContextNetwork _context;
        private readonly IAutorizacionServicesWorkProfile autorizacionServices;
        private readonly CloudinaryController _cloudinaryController;
        private readonly IConfiguration _config;

        public WorkProfileController(DbContextNetwork wpContext, IAutorizacionServicesWorkProfile autorizacionServices, CloudinaryController cloudinaryController, IConfiguration config)
        {
            _context = wpContext;
            this.autorizacionServices = autorizacionServices;
            _cloudinaryController = cloudinaryController;
            _config = config;

        }
        // //[Authorize]
        [HttpGet("Listado")]
        public async Task<List<WorkProfileWithRating>> Listado()        {
            var list = await _context.WorkProfiles
                .Select(w => new WorkProfileWithRating{
                    WorkId = w.WorkId,
                    Name = w.Name,
                    Email = w.Email,
                    Number = w.Number,
                    Province = w.Province,
                    City = w.City,
                    WorkDescription = w.WorkDescription,
                    ProfilePictureUrl = w.ProfilePictureUrl,
                    ProfileType = w.ProfileType,
                    AverageRating = _context.Reviews
                        .Where(r => r.WorkId == w.WorkId)
                        .Average(r => (double?)r.Rating) ?? 0
                })
                .ToListAsync();
            if (list == null)
            {
                return new List<WorkProfileWithRating>();
            }
            else
            {
                return list;
            }
        }

        [HttpPost("CrearCuenta")]
        public async Task<IActionResult> CrearCuentaAsync(WorkProfileImage workProfile)
        {
            bool workProfileExist = _context.WorkProfiles.Any(wp => wp.Email == workProfile.Email); //verifica si ya hay un WorkProfile con los mismos datos
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
                    var newWorkProfile = new WorkProfile
                    {
                        Name = workProfile.Name,
                        Email = workProfile.Email,
                        Number = workProfile.Number,
                        Password = workProfile.Password,
                        Province = workProfile.Province,
                        City = workProfile.City,
                        WorkDescription = workProfile.WorkDescription,
                        ProfilePictureUrl = imageUrl,
                        ProfileType = workProfile.ProfileType,
                        ImagePublicId = publicId

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
            }
        }

        ////[Authorize]
        [HttpGet("BuscarEmail")]
        public async Task<WorkProfile> Consultar(string email)
        {
            var temp = await _context.WorkProfiles.FirstOrDefaultAsync(wp => wp.Email == email);
            return temp;
        }

        ////[Authorize]
        [HttpGet("BuscarID")]
        public async Task<ActionResult<WorkProfileWithRating>> ConsultarID(int id)
        {
            var workProfile = await _context.WorkProfiles
                .Where(w => w.WorkId == id)
                .Select(w => new WorkProfileWithRating
                {
                    WorkId = w.WorkId,
                    Name = w.Name,
                    Email = w.Email,
                    Number = w.Number,
                    Province = w.Province,
                    City = w.City,
                    WorkDescription = w.WorkDescription,
                    ProfilePictureUrl = w.ProfilePictureUrl,
                    ProfileType = w.ProfileType,
                    AverageRating = _context.Reviews
                        .Where(r => r.WorkId == w.WorkId)
                        .Average(r => (double?)r.Rating) ?? 0
                })
                .FirstOrDefaultAsync();

            if (workProfile == null)
            {
                return NotFound();            }

            return workProfile;
        }

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
                else
                {
                    workerExist.ProfilePictureUrl = workerExist.ProfilePictureUrl ?? "ND";
                    workerExist.ImagePublicId = workerExist.ImagePublicId ?? "ND";
                }
                if (!string.IsNullOrEmpty(workProfile.Password) && workProfile.Password != workerExist.Password) //se verifica si la contraseña ha sido cambiada
                {
                    byte[] hashedPassword = HelperCryptography.EncriptarPassword(workProfile.Password, workerExist.Salt); // Si la contraseña ha sido modificada, se encripta
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
                var data = await _context.WorkProfiles.FirstOrDefaultAsync(wp => wp.Email == email);
                if (data != null)
                {
                    var listWorkSkills = await _context.WorkSkills.ToListAsync();
                    foreach (var ws in listWorkSkills) //se busca en la tabla workSkill todos los datos relacionados al workprofile y se eliminan
                    {
                        if (ws.WorkId == data.WorkId)
                        {
                            _context.WorkSkills.Remove(ws);
                            _context.SaveChanges();
                        }
                    }
                    await _cloudinaryController.DeleteImage(data.ImagePublicId); //se elimina la imagen de cloudinary
                    _context.WorkProfiles.Remove(data);
                    _context.SaveChanges();
                    msj = $"El perfil de {data.Name}, ha sido eliminado correctamente";
                }
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }

        // ***   MÉTODOS PARA RECUPERAR CONTRASEÑA    ***

        [HttpPost("Restablecer")]
        public async Task<string> RestablecerAsync(string email)
        {
            string msj = "";
            bool emailExist = _context.WorkProfiles.Any(wp => wp.Email.Equals(email)); //se verifica si existe un usuario con ese email
            if (emailExist)
            {
                WorkProfile workProfile = await _context.WorkProfiles.FirstOrDefaultAsync(wp => wp.Email == email);

                var token = GenerarTokenRestablecimiento(email, workProfile.WorkId.ToString(), workProfile.ProfileType.ToString());
                //enviar email
                await EnviarEmail(email, token);
                msj = "Correo enviado";
            }
            else
            {
                msj = "Este correo no se encuentra registrado";
            }
            return msj;
        }

        private string GenerarTokenRestablecimiento(string email, string workId, string profileType)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, email),
        new Claim("workId", workId.ToString()),
        new Claim("profileType", profileType),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(
                issuer: "NetworkApp",
                audience: "NetworkAppUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // El token expira en una hora
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


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
                }
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }

        private async Task<bool> EnviarEmail(String email, string token)
        {
            string mensaje = "";
            bool enviado = false;
            WorkProfile workProfile = await _context.WorkProfiles.FirstOrDefaultAsync(wp => wp.Email == email);
            try
            {
                EmailRestablecer emailRestablecer = new EmailRestablecer();
                emailRestablecer.Enviar(email, token);
                enviado = true;
                return enviado;
            }
            catch (Exception e)
            {
                mensaje = "Error al enviar el correo " + e.Message;
                return false;
            }
        }
        // ***   MÉTODO AUTENTICACION DE LOGIN    ***



        //Modificado para que sea por medio de un objeto y no con parametros escritos, asi mas facil y seguro de implementar en el front
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            //se busca al usuario por el email
            var worker = await _context.WorkProfiles.FirstOrDefaultAsync(w => w.Email.Equals(loginRequest.Email));
            if (worker == null)
            {
                return Unauthorized(); //si el usuario no existe
            }
            else
            {
                byte[] hashedPassword = HelperCryptography.EncriptarPassword(loginRequest.Password, worker.Salt); //se encriptar la contraseña con el mismo "salt" que está en la base de datos
                byte[] storedPassword = Convert.FromBase64String(worker.Password); //se convierte la contraseña almacenada (que está en Base64) a byte[] para compararla
                //se comparan las contraseñas encriptadas
                if (!HelperCryptography.CompareArrays(hashedPassword, storedPassword))
                {
                    return Unauthorized();
                }
                var autorizado = await autorizacionServices.DevolverToken(worker); //si la contraseña es correcta, generar el token
                if (autorizado == null)
                {
                    return Unauthorized();
                }
                else
                {
                    return Ok(autorizado);
                }
            }
        }
        public class WorkProfileWithRating
        {
            public int WorkId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Number { get; set; }
            public string Province { get; set; }
            public string City { get; set; }
            public string WorkDescription { get; set; }
            public string ProfilePictureUrl { get; set; }
            public char ProfileType { get; set; }
            public double AverageRating { get; set; }
        }

    }
}
