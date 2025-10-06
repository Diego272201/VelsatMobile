using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Model
{
    public class SimplifiedDevice
    {
        public string DeviceId { get; set; }
        public double LastValidLatitude { get; set; }
        public double LastValidLongitude { get; set; }
        public double LastValidSpeed {get; set; }
        public string Direccion { get; set; }
    }
}
