using API_Network.Models.Custom;
using API_Network.Models;

namespace API_Network.Services
{
    public interface IAutorizacionServicesWorkProfile
    {
        // Metodo encargado de devolver el Token al usuario autorizado
        Task<AutorizacionResponse> DevolverToken(WorkProfile autorizacion);
    }
}
