using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace API_Network.Models
{
    public class EmailRestablecer
    {
        //metodo para enviar el email
        public void Enviar (String correo, string workId)
        {
            try
            {
                string pathToTemplate = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "EmailRestablecer.html");
                string htmlBody = File.ReadAllText(pathToTemplate);

                //se crea la instancia del obj email 
                MailMessage email = new MailMessage();

                //asunto
                email.Subject = "Restablecer contraseña en Network";

                //destinatario
                email.To.Add(new MailAddress(correo));

                //emisor
                email.From = new MailAddress("networkapp.noreply@gmail.com");

                //html para el body del email
                htmlBody = htmlBody.Replace("{{Email}}", correo)
                    .Replace("{{workId}}", workId);

                //indicar que el contenido es en html
                email.IsBodyHtml = true;

                //prioridad
                email.Priority = MailPriority.Normal;

                //instanciar la vista del html para el body del email
                AlternateView view = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, MediaTypeNames.Text.Html);
                email.AlternateViews.Add(view);

                //agregar view al email
                SmtpClient smtp = new SmtpClient("smtp.gmail.com");

                // # puerto 
                smtp.Port = 587;

                //seguridad tipo SSL
                smtp.EnableSsl = true;

                //credencialess por default para el buzón de correo
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("networkapp.noreply@gmail.com", "stlx uoub lcbf ssby");

                //enviar email
                smtp.Send(email);

                //se liberan los recursos
                email.Dispose();
                smtp.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//end Enviar
    }
}
