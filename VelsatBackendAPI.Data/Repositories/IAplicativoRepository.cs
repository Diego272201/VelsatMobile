using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatMobile.Model;
using VelsatMobile.Model.NotificacionesMobile;

namespace VelsatMobile.Data.Repositories
{
    public interface IAplicativoRepository
    {
        Task<IEnumerable<Unidad>> GetUnidadesUsuario(string accountID);

        Task<IEnumerable<Notificaciones>> GetNotifications(string accountID);

        Task<double> GetKilometersDay(string deviceID);

        Task<IEnumerable<Notificaciones>> GetNotifDevice(string accountID, string deviceID);

        //Notificaciones
        Task<List<Documento>> DocumentosPorVencer(string accountID);

        Task<bool> SaveFCMTokenAsync(string userId, string fcmToken, string platform);

        Task<IEnumerable<UserFCMToken>> GetAllUserFCMTokensAsync();

        Task<int> ActualizarDeviceServicio(string deviceID, string codservicio);

        Task<int> ActualizarDeviceFinServicio(string deviceID);
    }
}
