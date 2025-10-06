using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelsatBackendAPI.Data
{
    public class MySqlConfiguration
    {
        public MySqlConfiguration(string defaultConnection, string secondConnection)
        {
            DefaultConnection = defaultConnection;
            SecondConnection = secondConnection;
        }

        public string DefaultConnection { get; set; }
        public string SecondConnection { get; set; }
    }
}
