using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatMobile.Model;

namespace VelsatMobile.Data.Repositories
{
    public class AplicativoRepository : IAplicativoRepository
    {
        private readonly IDbConnection _defaultConnection; IDbTransaction _defaultTransaction;

        public AplicativoRepository(IDbConnection defaultconnection, IDbTransaction defaulttransaction)
        {
            _defaultConnection = defaultconnection;
            _defaultTransaction = defaulttransaction;
        }

        //Método para obtener notificaciones de todo un usuario
        public async Task<IEnumerable<Notificaciones>> GetNotifications(string accountID)
        {
            string sql = @"Select accountID, deviceID, timestamp, statusCode, latitude, longitude, speedKPH from eventdata where accountID = @AccountID and timestamp >= UNIX_TIMESTAMP(CURDATE()) and statusCode IN (61714, 61715) order by timestamp DESC";

            return await _defaultConnection.QueryAsync<Notificaciones>(sql, new { AccountID = accountID});
        }

        //Método para obtener los kilómetros recorridos en un día
        public async Task<double> GetKilometersDay(string deviceID)
        {
            string sql = @"SELECT (MAX(odometerKM) - MIN(odometerKM)) as km FROM eventdata WHERE deviceID = @DeviceID AND timestamp >= UNIX_TIMESTAMP(CURDATE())";

            var result = await _defaultConnection.QueryFirstOrDefaultAsync<double?>(sql, new { DeviceID = deviceID });

            return result ?? 0;
        }

        //Método para obtener notificaciones de una unidad
        public async Task<IEnumerable<Notificaciones>> GetNotifDevice(string accountID, string deviceID)
        {
            string sql = @"Select accountID, deviceID, timestamp, statusCode, latitude, longitude, speedKPH from eventdata where accountID = @AccountID and deviceID = @DeviceID and timestamp >= UNIX_TIMESTAMP(CURDATE()) and statusCode IN (61714, 61715) order by timestamp DESC";

            return await _defaultConnection.QueryAsync<Notificaciones>(sql, new { AccountID = accountID, DeviceID = deviceID });
        }

        public async Task<IEnumerable<ServicioPasajero>> ServiciosPasajeros(string codcliente)
        {
            string fechaActual = DateTime.Now.ToString("dd/MM/yyyy");

            string sql = @"SELECT l.direccion, l.distrito, l.wy, l.wx, l.referencia, 
                          su.fecha as fechapasajero, su.orden, 
                          s.codservicio, s.empresa, s.numero, s.codconductor, 
                          s.destino, s.fecha as fechaservicio, s.tipo, 
                          s.totalpax, s.unidad, s.codusuario
                   FROM lugarcliente l, servicio s, subservicio su
                   WHERE l.codlugar = su.codubicli 
                     AND su.codservicio = s.codservicio 
                     AND su.codcliente = @Codcliente 
                     AND s.estado <> 'C' 
                     AND su.estado <> 'C'
                     AND s.fecha LIKE @Fecha";

            return await _defaultConnection.QueryAsync<ServicioPasajero>(sql, new { Codcliente = codcliente, Fecha = fechaActual + "%" });
        }

        public async Task<IEnumerable<DetalleDestino>> GetDetalleDestino(string codcliente)
        {
            string sql = @"SELECT c.apellidos, c.nombres, l.direccion, l.distrito, l.wy, l.wx FROM cliente c, lugarcliente l WHERE c.codlugar = l.codcli AND c.codcliente = @Codcliente AND l.estado = 'A'";

            return await _defaultConnection.QueryAsync<DetalleDestino>(sql, new { Codcliente = codcliente });
        }

        public async Task<DetalleConductor> GetDetalleConductor(string codtaxi)
        {
            string sql = @"SELECT apellidos, nombres, telefono, dni, calificacion FROM taxi WHERE codtaxi = @Codtaxi";

            return await _defaultConnection.QueryFirstOrDefaultAsync<DetalleConductor>(sql, new { Codtaxi = codtaxi });
        }
    }
}
