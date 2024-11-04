using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace API_Network.Models
{
    public class EmailRestablecer
    {
        private readonly IConfiguration _configuration;

        public EmailRestablecer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Método para enviar el email
        public void Enviar(string correo, string token)
        {
            try
            {
                string pathToTemplate = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "EmailRestablecer.html");
                string htmlBody = File.ReadAllText(pathToTemplate);

                MailMessage email = new MailMessage
                {
                    Subject = "Restablecer contraseña en Network",
                    IsBodyHtml = true,
                    Priority = MailPriority.Normal
                };

                email.To.Add(new MailAddress(correo));
                email.From = new MailAddress(_configuration["SmtpSettings:Username"]);
                htmlBody = htmlBody.Replace("{{Email}}", correo)
                                   .Replace("{{Token}}", token);

                AlternateView view = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, MediaTypeNames.Text.Html);
                email.AlternateViews.Add(view);

                SmtpClient smtp = new SmtpClient
                {
                    Host = _configuration["SmtpSettings:Host"],
                    Port = int.Parse(_configuration["SmtpSettings:Port"]),
                    EnableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"]),
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        _configuration["SmtpSettings:Username"],
                        _configuration["SmtpSettings:Password"]
                    )
                };

                smtp.Send(email);
                email.Dispose();
                smtp.Dispose();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al enviar el correo", ex);
            }
        }
    }
}
