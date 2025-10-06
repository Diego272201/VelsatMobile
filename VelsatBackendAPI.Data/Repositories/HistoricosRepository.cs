using Dapper;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities.Net;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Data.Repositories
{
    public class HistoricosRepository : IHistoricosRepository
    {
        private readonly IDbConnection _defaultConnection;
        private readonly IDbConnection _secondConnection;
        private readonly IDbTransaction _defaultTransaction;

        public HistoricosRepository(IDbConnection defaultConnection, IDbConnection secondConnection, IDbTransaction defaulttransaction)
        {
            _defaultConnection = defaultConnection;
            _secondConnection = secondConnection;
            _defaultTransaction = defaulttransaction;

        }

        public async Task<DatosReporting> GetDataReporting(string fechaini, string fechafin, string deviceID, string accountID)
        {
            // Paso 1: Buscar accountID en device
            const string sqlAccountFromDevice = "SELECT accountID FROM device WHERE deviceID = @DeviceID";
            var newAccountID =  _defaultConnection.QueryFirstOrDefault<string>(sqlAccountFromDevice, new { DeviceID = deviceID }, transaction: _defaultTransaction);

            if (!string.IsNullOrEmpty(newAccountID))
            {
                accountID = newAccountID;
            }
            else
            {

                // Validar que tengamos un accountID válido
                if (string.IsNullOrEmpty(accountID))
                {
                    return new DatosReporting
                    {
                        Mensaje = $"No se encontró información para el deviceID: {deviceID}"
                    };
                }
            }

            var dates = FormatDate(fechaini, fechafin);
            fechaini = dates.dateStart;
            fechafin = dates.dateEnd;
            var resultadoDias = CalcularDias(fechaini, fechafin);
            double numdias = resultadoDias.NumDias;

            if (numdias <= 3)
            {
                const string sql = "select tabla from historicos where timeini<=@FechafinUnix and timefin>=@FechainiUnix";

                var nombresTablas = _defaultConnection.Query<Historicos>(sql, new { FechainiUnix = resultadoDias.UnixFechaInicio, FechafinUnix = resultadoDias.UnixFechaFin }, transaction: _defaultTransaction).ToList();

                var datosReporting = new DatosReporting
                {
                    ListaTablas = new List<TablasReporting>()
                };

                if (nombresTablas.Count == 0)
                {
                    // Si no se encontraron nombres de tablas, consultamos directamente la tabla "eventdata"
                    string sqlEventData = @"
                        SELECT deviceID, timestamp, speedKPH, longitude, latitude, odometerKM, address 
                         FROM eventdata 
                            WHERE accountID = @AccountID AND deviceID = @DeviceID AND timestamp BETWEEN @FechainiUnix AND @FechafinUnix
                                ORDER BY timestamp";

                    datosReporting.ListaTablas = _defaultConnection.Query<TablasReporting>(sqlEventData, new { AccountID = accountID,  DeviceID = deviceID, FechainiUnix = resultadoDias.UnixFechaInicio, FechafinUnix = resultadoDias.UnixFechaFin }, transaction: _defaultTransaction).ToList();

                    for (int i = 0; i < datosReporting.ListaTablas.Count; i++)
                    {
                        datosReporting.ListaTablas[i].Item = i + 1;
                    }

                }

                else

                {
                    foreach (var nombreTabla in nombresTablas)
                    {
                        string consultaTabla = nombreTabla.Tabla;

                        string sqlR = $@"
                           SELECT deviceID, timestamp, speedKPH, longitude, latitude, odometerKM, address 
                                FROM {consultaTabla} 
                                    WHERE accountID = @AccountID AND deviceID = @DeviceID AND timestamp BETWEEN @FechainiUnix AND @FechafinUnix
                                      ORDER BY timestamp";

                        var datosTabla = _secondConnection.Query<TablasReporting>(sqlR, new { AccountID = accountID, DeviceID = deviceID, FechainiUnix = resultadoDias.UnixFechaInicio, FechafinUnix = resultadoDias.UnixFechaFin }, transaction: _defaultTransaction).ToList();

                        datosReporting.ListaTablas.AddRange(datosTabla);

                    }

                    for (int i = 0; i < datosReporting.ListaTablas.Count; i++)
                    {
                        datosReporting.ListaTablas[i].Item = i + 1;
                    }
                }

                if (datosReporting.ListaTablas.Count == 0 || datosReporting.ListaTablas == null)
                {
                    return new DatosReporting
                    {
                        Mensaje = "No se encontro datos disponible en el rango de fechas ingresado"
                    };
                }

                return datosReporting;
            }
            else
            {
                return new DatosReporting
                {
                    Mensaje = "La diferencia entre las fechas es mayor a 3 días; seleccione otra fechas"
                };
            }
        }


        public int DateUnix(string fecha)
        {
            fecha = WebUtility.UrlDecode(fecha);

            DateTime fechaTime = DateTime.ParseExact(fecha, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

            int unixFecha = (int)(fechaTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;

            return unixFecha;
        }

        public class ResultadosCalculoDias
        {
            public double NumDias { get; set; }
            public int UnixFechaInicio { get; set; }
            public int UnixFechaFin { get; set; }
        }

        public ResultadosCalculoDias CalcularDias(string fechaI, string fechaF)
        {
            int fechainiUnix = DateUnix(fechaI);
            int fechafinUnix = DateUnix(fechaF);

            int totalsegundos = fechafinUnix - fechainiUnix;
            double numdias = (double)totalsegundos / 86400;

            return new ResultadosCalculoDias
            {
                NumDias = numdias,
                UnixFechaInicio = fechainiUnix,
                UnixFechaFin = fechafinUnix,
            };
        }

        public async Task<List<SpeedReporting>> GetSpeedData(string fechaini, string fechafin, string deviceID, double speedKPH, string accountID)
        {
            var datosreporte = await GetDataReporting(fechaini, fechafin, deviceID, accountID);

            List<SpeedReporting> SpeedData = new List<SpeedReporting>();

            if (datosreporte != null && datosreporte.ListaTablas.Count > 0)
            {
                for (int i = 0; i < datosreporte.ListaTablas.Count - 1; i++)
                {
                    if (datosreporte.ListaTablas[i].SpeedKPH >= speedKPH)
                    {
                        SpeedReporting data = new SpeedReporting
                        {
                            Item = SpeedData.Count + 1,
                            SpeedKPH = datosreporte.ListaTablas[i].SpeedKPH,
                            Date = datosreporte.ListaTablas[i].Fecha,
                            Time = datosreporte.ListaTablas[i].Hora,
                            Latitude = datosreporte.ListaTablas[i].Latitude,
                            Longitude = datosreporte.ListaTablas[i].Longitude,
                            Address = datosreporte.ListaTablas[i].Address
                        };
                        SpeedData.Add(data);
                    }
                }
            }
            return SpeedData;
        }

        public async Task<List<StopsReporting>> GetStopData(string fechaini, string fechafin, string deviceID, string accountID)
        {
            var datosreporte = await GetDataReporting(fechaini, fechafin, deviceID, accountID);

            List<StopsReporting> StopsData = new List<StopsReporting>();

            if (datosreporte != null && datosreporte.ListaTablas.Count > 0)
            {
                int Contador = 0;
                int PuntoFinal = 0;

                for (int i = 0; i < datosreporte.ListaTablas.Count - 1; i++)
                {
                    if (datosreporte.ListaTablas[i].SpeedKPH == 0 && datosreporte.ListaTablas[i + 1].SpeedKPH == 0)
                    {
                        Contador++;

                        int ultimoelemento = datosreporte.ListaTablas.Count - 1;

                        if ((i + 1) == ultimoelemento && Contador > 0)
                        {
                            PuntoFinal = i + 1;

                            StopsReporting stop = new StopsReporting
                            {
                                Item = StopsData.Count + 1,
                                StartDate = datosreporte.ListaTablas[PuntoFinal - Contador].Fecha,
                                StartTime = datosreporte.ListaTablas[PuntoFinal - Contador].Hora,
                                EndDate = datosreporte.ListaTablas[PuntoFinal].Fecha,
                                EndTime = datosreporte.ListaTablas[PuntoFinal].Hora,
                                Longitude = datosreporte.ListaTablas[PuntoFinal - Contador].Longitude,
                                Latitude = datosreporte.ListaTablas[PuntoFinal - Contador].Latitude,
                                Address = datosreporte.ListaTablas[PuntoFinal].Address,
                                TimeStampIni = datosreporte.ListaTablas[PuntoFinal - Contador].Timestamp,
                                TimeStampEnd = datosreporte.ListaTablas[PuntoFinal].Timestamp
                            };

                            int diferenciaEnSegundos = stop.TimeStampEnd - stop.TimeStampIni;

                            int horas = diferenciaEnSegundos / 3600;
                            int minutos = (diferenciaEnSegundos % 3600) / 60;
                            int segundos = diferenciaEnSegundos % 60;

                            string totalTime = $"{horas:D2}H:{minutos:D2}M:{segundos:D2}S";

                            stop.TotalTime = totalTime;

                            StopsData.Add(stop);
                            Contador = 0;
                        }
                    }
                    else
                    {
                        if (Contador > 0)
                        {
                            StopsReporting stop = new StopsReporting
                            {
                                Item = StopsData.Count + 1,
                                StartDate = datosreporte.ListaTablas[i - Contador].Fecha,
                                StartTime = datosreporte.ListaTablas[i - Contador].Hora,
                                EndDate = datosreporte.ListaTablas[i].Fecha,
                                EndTime = datosreporte.ListaTablas[i].Hora,
                                Longitude = datosreporte.ListaTablas[i - Contador].Longitude,
                                Latitude = datosreporte.ListaTablas[i - Contador].Latitude,
                                Address = datosreporte.ListaTablas[i].Address,
                                TimeStampIni = datosreporte.ListaTablas[i - Contador].Timestamp,
                                TimeStampEnd = datosreporte.ListaTablas[i].Timestamp
                            };

                            int diferenciaEnSegundos = stop.TimeStampEnd - stop.TimeStampIni;

                            int horas = diferenciaEnSegundos / 3600;
                            int minutos = (diferenciaEnSegundos % 3600) / 60;
                            int segundos = diferenciaEnSegundos % 60;

                            string totalTime = $"{horas:D2}H:{minutos:D2}M:{segundos:D2}S";

                            stop.TotalTime = totalTime;

                            StopsData.Add(stop);
                            Contador = 0;
                        }
                    }
                }
            }
            return StopsData;
        }

        public class ResultDate
        {
            public string dateStart { get; set; }
            public string dateEnd { get; set; }

        }

        public ResultDate FormatDate(string dateS, string dateE)
        {

            DateTime.TryParse(dateS, out DateTime fechaInicio);
            DateTime.TryParse(dateE, out DateTime fechaFin);


            string fechaInicioString = fechaInicio.ToString("dd/MM/yyyy HH:mm");
            string fechaFinString = fechaFin.ToString("dd/MM/yyyy HH:mm");

            return new ResultDate
            {
                dateStart = fechaInicioString,
                dateEnd = fechaFinString,
            };
        }

        public async Task<List<RouteDetails>> GetRouteDetails(string fechaini, string fechafin, string deviceID, string accountID)
        {
            var datareport = await GetDataReporting(fechaini, fechafin, deviceID, accountID);

            List<RouteDetails> DetailsData = new List<RouteDetails>();

            if (datareport != null && datareport.ListaTablas.Count > 0)
            {
                int Contador = 0;
                int PuntoFinal = 0;

                int ultimoelemento = datareport.ListaTablas.Count - 1;

                for (int i = 0; i < datareport.ListaTablas.Count - 1; i++)
                {
                    if (datareport.ListaTablas[i].SpeedKPH == 0 && datareport.ListaTablas[i + 1].SpeedKPH == 0)
                    {
                        Contador++;


                        if ((i + 1) == ultimoelemento && Contador > 0)
                        {
                            PuntoFinal = i + 1;

                            RouteDetails stop = CreateRouteDetails(datareport.ListaTablas[PuntoFinal - Contador]);

                            DetailsData.Add(stop);
                            Contador = 0;
                        }
                    }
                    else
                    {
                        if (Contador == 0)
                        {
                            RouteDetails stop = CreateRouteDetails(datareport.ListaTablas[i]);
                            DetailsData.Add(stop);
                            Contador = 0;
                        }


                        if (Contador > 0)
                        {
                            RouteDetails stop = CreateRouteDetails(datareport.ListaTablas[i - Contador]);

                            DetailsData.Add(stop);
                            Contador = 0;
                        }

                        if ((i + 1) == ultimoelemento && datareport.ListaTablas[ultimoelemento].SpeedKPH > 0)
                        {
                            RouteDetails stop = CreateRouteDetails(datareport.ListaTablas[ultimoelemento]);
                            DetailsData.Add(stop);
                            Contador = 0;
                        }
                    }
                }
            }
            return DetailsData;
        }

        private RouteDetails CreateRouteDetails(TablasReporting gpsData)
        {
            return new RouteDetails
            {
                Date = gpsData.Fecha,
                Time = gpsData.Hora,
                Speed = gpsData.SpeedKPH,
                Longitude = gpsData.Longitude,
                Latitude = gpsData.Latitude,

            };
        }

        public string UserName(string deviceID)
        {
            const string sql = "select accountID from device where deviceID = @DeviceID";

            string account = _defaultConnection.QueryFirstOrDefault<string>(sql, new { DeviceID = deviceID }, transaction: _defaultTransaction);

            const string sqlUser = "Select description from account where accountID = @AccountId";

            string userName = _defaultConnection.QueryFirstOrDefault<string>(sqlUser, new { AccountId = account }, transaction: _defaultTransaction);

            return userName;
        }

        public async Task<List<string>> DeviceFilterSedapal(string rutadefault)
        {
            string sql = "Select deviceID from device where accountID = 'sedapal' and rutadefault = @Rutadefault";

            var parameters = new { Rutadefault = rutadefault };

            var result = await _defaultConnection.QueryAsync<string>(sql, parameters);

            return result.ToList();
        }
    }
}