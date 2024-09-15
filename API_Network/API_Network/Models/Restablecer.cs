using System.ComponentModel.DataAnnotations;

namespace API_Network.Models
{
    public class Restablecer
    {
        [Required(ErrorMessage = "Debe ingresar su nueva contraseña")]
        public string NuevoPassword { get; set; }

        [Required(ErrorMessage = "Confirme su nueva contraseña")]
        public string Confirmar { get; set; }
    }
}
