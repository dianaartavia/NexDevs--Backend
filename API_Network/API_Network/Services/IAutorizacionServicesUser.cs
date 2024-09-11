
using API_Network.Models;
using API_Network.Models.Custom;

namespace API_Network.Services
{

    public interface IAutorizacionServicesUser
    {
        // Metodo encargado de devolver el Token al usuario autorizado
        Task<AutorizacionResponse> DevolverToken(User autorizacion);
    }
}
