using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatMobile.Model;

namespace VelsatMobile.Data.Repositories
{
    public interface IAplicativoRepository
    {
        Task<IEnumerable<Notificaciones>> GetNotifications(string accountID);

        Task<double> GetKilometersDay(string deviceID);

        Task<IEnumerable<Notificaciones>> GetNotifDevice(string accountID, string deviceID);

        Task<IEnumerable<ServicioPasajero>> ServiciosPasajeros(string codcliente);

        Task<IEnumerable<DetalleDestino>> GetDetalleDestino(string codcliente);

        Task<DetalleConductor> GetDetalleConductor(string codtaxi);

    }
}
