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
    public class UsersController : Controller
    {
        private readonly DbContextNetwork _context;
        private readonly IAutorizacionServicesUser autorizacionServices;

        private readonly CloudinaryController _cloudinaryController;
        private readonly IConfiguration _config;

        public UsersController(DbContextNetwork uContext, IAutorizacionServicesUser autorizacionServices, CloudinaryController cloudinaryController, IConfiguration config)
        {
            _context = uContext;
            this.autorizacionServices = autorizacionServices;
            _cloudinaryController = cloudinaryController;
            _config = config;
        }

        //[Authorize]
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
            }
        }

        [HttpPost("CrearCuenta")]
        public async Task<IActionResult> CrearCuenta(UserImage user)
        {
            bool userExist = _context.Users.Any(u => u.Email == user.Email); //verifica si ya hay un User con los mismos datos
            var imageUrl = "";
            var publicId = "";
            try
            {
                if (!userExist)
                {
                    if (user.ProfilePictureUrl != null)
                    {
                        var result = await _cloudinaryController.SaveImage(user.ProfilePictureUrl, "users"); // Llamar al método de subida de imagen

                        if (result is OkObjectResult okResult)
                        {
                            var uploadResult = okResult.Value as dynamic; // Extraer los valores de la respuesta
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
                        ProfileType = user.ProfileType,
                        ImagePublicId = publicId
                    };
                    newUser.Salt = HelperCryptography.GenerateSalt();
                    byte[] hashedPassword = HelperCryptography.EncriptarPassword(newUser.Password, newUser.Salt);
                    newUser.Password = Convert.ToBase64String(hashedPassword);

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
            }
        }

        //[Authorize]
        [HttpGet("BuscarEmail")]
        public async Task<User> Consultar(string email)
        {
            var temp = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return temp;
        }

        //[Authorize]
        [HttpPut("Editar")]
        public async Task<string> Editar(UserImage user)
        {
            string msj = "Error al editar el perfil";
            var userExist = _context.Users.FirstOrDefault(u => u.UserId == user.UserId);
            try
            {
                if (user.ProfilePictureUrl != null && user.ProfilePictureUrl.Length > 0)
                {
                    var tempPublicId = userExist.ImagePublicId;
                    var result = await _cloudinaryController.SaveImage(user.ProfilePictureUrl, "users");
                    if (result is OkObjectResult okResult)
                    {
                        var uploadResult = okResult.Value as dynamic;
                        if (uploadResult != null)
                        {
                            await _cloudinaryController.DeleteImage(tempPublicId);
                            userExist.ProfilePictureUrl = uploadResult.Url;
                            userExist.ImagePublicId = uploadResult.PublicId;
                        }
                    }
                }
                else
                {
                    userExist.ProfilePictureUrl = userExist.ProfilePictureUrl ?? "ND";
                    userExist.ImagePublicId = userExist.ImagePublicId ?? "ND";
                }
                if (!string.IsNullOrEmpty(user.Password) && user.Password != userExist.Password) //se verifica si la contraseña ha sido cambiada
                {
                    byte[] hashedPassword = HelperCryptography.EncriptarPassword(user.Password, userExist.Salt); //si la contraseña ha sido modificada, se encripta
                    userExist   .Password = Convert.ToBase64String(hashedPassword);
                }
                // Actualizar los demás campos del perfil con los datos recibidos de UserImage
                userExist.FirstName = user.FirstName; 
                userExist.LastName = user.LastName;
                userExist.Email = user.Email;
                userExist.Province = user.Province;
                userExist.City = user.City;
                userExist.Bio = user.Bio;
                userExist.ProfileType = user.ProfileType;
                _context.Users.Update(userExist);
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
                var data = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (data != null)
                {
                    await _cloudinaryController.DeleteImage(data.ImagePublicId); //se elimina la imagen de cloudinary
                    _context.Users.Remove(data);
                    _context.SaveChanges();
                    msj = $"El perfil de {data.FirstName} {data.LastName}, ha sido eliminado correctamente";
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
            bool emailExist = _context.Users.Any(u => u.Email.Equals(email));//se verifica si existe un usuario con ese email
            if (emailExist)
            {
                User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                var token = GenerarTokenRestablecimiento(email, user.UserId.ToString(), user.ProfileType.ToString());
                //Se envía el correo con el link para restablecer la contraseña
                await EnviarEmail(email, token);
                msj = "Correo enviado";
            }
            else
            {
                msj = "Este correo no se encuentra registrado";
            }
            return msj;
        }

        private string GenerarTokenRestablecimiento(string email, string userId, string profileType)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, email),
        new Claim("userId", userId.ToString()),
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
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
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
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(loginRequest.Email)); //se busca al usuario por el email
            if (user == null)
            {
                return Unauthorized(); //si el usuario no existe
            }
            else
            {
                byte[] hashedPassword = HelperCryptography.EncriptarPassword(loginRequest.Password, user.Salt); //se encriptar la contraseña con el mismo "salt" que está en la base de datos
                byte[] storedPassword = Convert.FromBase64String(user.Password); //se convierte la contraseña almacenada (que está en Base64) a byte[] para compararla
                if (!HelperCryptography.CompareArrays(hashedPassword, storedPassword)) //se comparan las contraseñas encriptadas
                {
                    return Unauthorized();
                }
                var autorizado = await autorizacionServices.DevolverToken(user); //si la contraseña es correcta, generar el token
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
    }
}
