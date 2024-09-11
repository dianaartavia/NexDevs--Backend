using API_Network.Models;
using API_Network.Models.Custom;
using API_Network.Context;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Network.Services
{
    public class AutorizacionServicesUser : IAutorizacionServicesUser
    {
        //Variable para utilizar el archivo appsettings.json
        private readonly IConfiguration _configuration;
        //variable para utilizar las funciones ORM
        private readonly DbContextNetwork _context;

        public AutorizacionServicesUser(IConfiguration config, DbContextNetwork context)
        {
            _configuration = config;
            _context = context;
        }

        //Método encargado de dar autorizacion a un User y
        //enviarle su access token para el uso de los
        //métodos publiacdo por la API
        public async Task<AutorizacionResponse> DevolverToken(User autorizacion)
        {
            var temp = await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(autorizacion.Email) && u.Password.Equals(autorizacion.Password));
            if (temp == null) 
            { 
                return await Task.FromResult<AutorizacionResponse>(null);
            }

            string tokenCreado = GenerarToken(autorizacion.Email.ToString());

            return new AutorizacionResponse() { Token = tokenCreado, Resultado = true, Msj = "Ok"};
        }

        //Método encargado de generar el token
        private string GenerarToken(string UserId)
        {
            var key = _configuration.GetValue<string>("JwtSettings:Key");
            var keyBytes = Encoding.ASCII.GetBytes(key);

            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, UserId));

            var credencialesToken = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256Signature
                );

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = credencialesToken
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);

            var tokenCreado = tokenHandler.WriteToken(tokenConfig);

            return tokenCreado;
        }
    }
}