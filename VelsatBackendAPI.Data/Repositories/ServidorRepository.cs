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
        private readonly IDbTransaction _defaultTransaction;

        public ServidorRepository(IDbConnection defaultconnection, IDbTransaction defaulttransaction)
        {
            _defaultConnection = defaultconnection;
            _defaultTransaction = defaulttransaction;
        }

        public async Task<Servidor> GetServidor(string accountID)
        {
            const string sql = "Select * from servermobile where loginusu = @AccountID";
            return await _defaultConnection.QueryFirstOrDefaultAsync<Servidor>(sql, new { AccountID = accountID }, transaction: _defaultTransaction);
        }
    }
}
