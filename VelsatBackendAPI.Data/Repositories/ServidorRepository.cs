using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Data.Repositories
{
    public class ServidorRepository : IServidorRepository
    {
        private readonly IDbConnection _defaultConnection;

        public ServidorRepository(IDbConnection defaultconnection)
        {
            _defaultConnection = defaultconnection;
        }

        public async Task<Servidor> GetServidor(string accountID)
        {
            const string sql = "Select * from servermobile where loginusu = @AccountID";
            return await _defaultConnection.QueryFirstOrDefaultAsync<Servidor>(sql, new { AccountID = accountID });
        }
    }
}
