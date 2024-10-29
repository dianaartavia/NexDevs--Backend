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
                MailMessage email = new MailMessage(); //se crea la instancia del obj email 
                email.Subject = "Restablecer contraseña en Network"; //asunto
                email.To.Add(new MailAddress(correo)); //destinatario
                email.From = new MailAddress("networkapp.noreply@gmail.com"); //emisor
                htmlBody = htmlBody.Replace("{{Email}}", correo) //html para el body del email
                    .Replace("{{workId}}", workId);
                email.IsBodyHtml = true; //indicar que el contenido es en html
                email.Priority = MailPriority.Normal; //prioridad
                //instanciar la vista del html para el body del email
                AlternateView view = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, MediaTypeNames.Text.Html);
                email.AlternateViews.Add(view);
                SmtpClient smtp = new SmtpClient("smtp.gmail.com"); //agregar view al email
                smtp.Port = 587; //agregar view al email
                smtp.EnableSsl = true; //seguridad tipo SSL
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
        }
    }
}
