using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model
{
    public class Device
    {
        public string DeviceId { get; set; }
        public double? LastGPSTimestamp { get; set; }
        public double LastValidLatitude { get; set; }
        public double LastValidLongitude { get; set; }
        public double LastValidHeading { get; set; }      
        public double LastValidSpeed { get; set; }
        public double LastOdometerKM { get; set; }
        public double? Odometerini { get; set; }
        public double? Kmini { get; set; }
        public string? Descripcion { get; set; }
        public string Direccion { get; set; }
        public string? Codgeoact { get; set; }
        public string? Rutaact { get; set; }
        public string? Servicio { get; set; }
        public Geocercausu? DatosGeocercausu { get; set; }

    }
}
