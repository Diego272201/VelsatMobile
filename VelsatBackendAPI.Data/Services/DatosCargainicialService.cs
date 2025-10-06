using Dapper;
using MySql.Data.MySqlClient;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Model;
using VelsatMobile.Model;

namespace VelsatBackendAPI.Data.Services
{
    public class DatosCargainicialService : IDatosCargainicialService
    {
        private readonly IDbConnection _defaultConnection;
        private readonly IDbTransaction _defaultTransaction;
        private List<Geocercausu> _geocercaUsuarios;
        private List<string> _deviceIds; // Variable global para los deviceIDs
        private string _currentLogin; // Para trackear si cambió el usuario

        public DatosCargainicialService(IDbConnection defaultconnection, IDbTransaction defaulttransaction)
        {
            _defaultConnection = defaultconnection;
            _defaultTransaction = defaulttransaction;
            _geocercaUsuarios = _defaultConnection.Query<Geocercausu>("SELECT codigo, descripcion, latitud, longitud FROM geocercausu", transaction: _defaultTransaction).ToList();
        }

        // Método para cargar los deviceIDs una sola vez por usuario
        private async Task<List<string>> GetDeviceIdsAsync(string login)
        {
            // Si ya tenemos los IDs para este usuario, los devolvemos
            if (_deviceIds != null && _currentLogin == login)
            {
                return _deviceIds;
            }

            // Cargar los deviceIDs
            const string sqlGetAllDeviceIds = @"
                SELECT DISTINCT deviceID 
                FROM (
                    SELECT deviceID FROM device WHERE accountID = @Login 
                    UNION ALL 
                    SELECT deviceID FROM gts.deviceuser WHERE userID = @Login AND Status = 1
                ) AS combined_devices";

            _deviceIds = (await _defaultConnection.QueryAsync<string>(sqlGetAllDeviceIds,
                new { Login = login }, transaction: _defaultTransaction)).ToList();
            _currentLogin = login;

            return _deviceIds;
        }

        public Geocercausu ObtenerGeocercausuPorCodigo(string codigo)
        {
            var geocercausu = _geocercaUsuarios.FirstOrDefault(gu => gu.Codigo.ToString() == codigo);
            return geocercausu;
        }

        public async Task<DatosCargainicial> ObtenerDatosCargaInicialAsync(string login)
        {
            // Obtener los deviceIDs (desde cache o BD)
            var deviceIds = await GetDeviceIdsAsync(login);

            if (!deviceIds.Any())
            {
                return new DatosCargainicial
                {
                    FechaActual = DateTime.Now,
                    DatosDevice = new List<Device>()
                };
            }

            // PRIMERA CONSULTA: Dispositivos
            const string sqlGetDevices = @"SELECT deviceID, lastGPSTimestamp, lastValidLatitude, lastValidLongitude, lastOdometerKM, direccion, lastValidHeading, lastValidSpeed FROM device WHERE deviceID IN @DeviceIDs";

            var devices = (await _defaultConnection.QueryAsync<Device>(sqlGetDevices,
                new { DeviceIDs = deviceIds }, transaction: _defaultTransaction)).ToList();

            // Asignar geocercas
            foreach (var device in devices)
            {
                device.DatosGeocercausu = ObtenerGeocercausuPorCodigo(device.Codgeoact);
            }

            var datosCargaInicial = new DatosCargainicial
            {
                FechaActual = DateTime.Now,
                DatosDevice = devices
            };

            return datosCargaInicial;
        }

        public async Task<IEnumerable<SimplifiedDevice>> SimplifiedList(string login)
        {
            // Obtener los deviceIDs (desde cache o BD)
            var deviceIds = await GetDeviceIdsAsync(login);

            if (!deviceIds.Any())
            {
                return new List<SimplifiedDevice>();
            }

            // Consulta optimizada usando los IDs en cache
            const string sqlGetSimplifiedDevices = @"SELECT DISTINCT d.deviceID, d.lastValidLatitude, d.lastValidLongitude, d.lastValidSpeed, d.direccion FROM device d WHERE d.deviceID IN @DeviceIds";

            var listDevices = await _defaultConnection.QueryAsync<SimplifiedDevice>(
                sqlGetSimplifiedDevices,
                new { DeviceIds = deviceIds },
                transaction: _defaultTransaction);

            return listDevices;
        }

        public async Task<DatosVehiculo> ObtenerDatosVehiculoAsync(string login, string placa)
        {
            // Obtener los deviceIDs del usuario para validar que le pertenece
            var deviceIds = await GetDeviceIdsAsync(login);

            if (!deviceIds.Any())
            {
                return new DatosVehiculo
                {
                    FechaActual = DateTime.Now,
                    Vehiculo = null
                };
            }

            const string sqlGetVehicle = @"SELECT deviceID, lastGPSTimestamp, lastValidLatitude, lastValidLongitude, lastValidHeading, lastValidSpeed, lastOdometerKM, direccion FROM device WHERE deviceID IN @DeviceIDs AND deviceID = @Placa";

            var vehiculo = await _defaultConnection.QueryFirstOrDefaultAsync<Device>(
                sqlGetVehicle,
                new { DeviceIDs = deviceIds, Placa = placa },
                transaction: _defaultTransaction
            );

            return new DatosVehiculo
            {
                FechaActual = DateTime.Now,
                Vehiculo = vehiculo
            };
        }

        // Método opcional para limpiar el cache si es necesario
        public void ClearDeviceIdsCache()
        {
            _deviceIds = null;
            _currentLogin = null;
        }
    }
}