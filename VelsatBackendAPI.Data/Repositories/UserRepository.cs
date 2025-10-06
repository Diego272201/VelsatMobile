using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Model;

namespace VelsatBackendAPI.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _defaultConnection; IDbTransaction _defaultTransaction;

        public UserRepository(IDbConnection defaultconnection, IDbTransaction defaulttransaction)
        {
            _defaultConnection = defaultconnection;
            _defaultTransaction = defaulttransaction;
        }

        /* private readonly MySqlConfiguration _configuration;
         public UserRepository(MySqlConfiguration configuration)
         {
             _configuration = configuration;
         }

         protected MySqlConnection dbconnection()
         {
             return new MySqlConnection(_configuration.ConnectionString);
         }*/

        public async Task<Account> GetDetails(string accountID, char tipo)
        {
            string sql = tipo switch
            {
                'n' => @"SELECT description, ruc, contactEmail, contactPhone FROM usuarios WHERE accountID = @AccountID",

                'p' => @"SELECT apellidos, dni, telefono, codlan, empresa FROM cliente WHERE codlan = @AccountID",

                'c' => @"SELECT apellidos, nombres, dni, telefono, login FROM taxi WHERE login = @AccountID",

                _ => throw new ArgumentException("Tipo de usuario no válido", nameof(tipo))
            };

            return await _defaultConnection.QueryFirstOrDefaultAsync<Account>(sql, new { AccountID = accountID });
        }

        public async Task<bool> UpdateUser(Account account, string username, char tipo)
        {
            string sql = tipo switch
            {
                'n' => @"UPDATE usuarios SET description = @Description, contactEmail = @ContactEmail, contactPhone = @ContactPhone WHERE accountID = @AccountID",

                'p' => @"UPDATE cliente SET apellidos = @Apellidos, codlan = @Codlan, telefono = @Telefono WHERE codlan = @AccountID",

                'c' => @"UPDATE taxi SET apellidos = @Apellidos, login = @Login, telefono = @Telefono WHERE login = @AccountID",

                _ => throw new ArgumentException("Tipo de usuario no válido", nameof(tipo))
            };

            var parametres = new
            {
                Description = account.Description,
                ContactEmail = account.ContactEmail,
                ContactPhone = account.ContactPhone,
                Apellidos = account.Apellidos,
                Codlan = account.Codlan,
                Telefono = account.Telefono,
                Login = account.Login,
                AccountID = username,
            };

            var rowsAffected = await _defaultConnection.ExecuteAsync(sql, parametres);

            return rowsAffected > 0;
        }

        public async Task<bool> UpdatePassword(string username, string password, char tipo)
        {
            string sql = tipo switch
            {
                'n' => @"UPDATE usuarios SET password = @Password WHERE accountID = @AccountID",

                'p' => @"UPDATE cliente SET clave = @Clave WHERE codlan = @AccountID",

                'c' => @"UPDATE taxi SET clave = @Clave WHERE login = @AccountID",

                _ => throw new ArgumentException("Tipo de usuario no válido", nameof(tipo))
            };

            var parametres = new
            {
                AccountID = username,
                Password = password,
                Clave = password
            };

            var rowsAffected = await _defaultConnection.ExecuteAsync(sql, parametres);

            return rowsAffected > 0;
        }

        public async Task<Account> ValidateUser(string login, string clave, char tipo)
        {
            string sql;

            switch (tipo)
            {
                case 'n':
                    sql = "SELECT accountID, password, description FROM usuarios WHERE accountID = @Login AND password = @Clave";
                    break;
                case 'p':
                    sql = "SELECT codcliente as Codigo, codlan as AccountID, clave as Password, apellidos as Description FROM cliente WHERE codlan = @Login AND clave = @Clave";
                    break;
                case 'c':
                    sql = "SELECT codtaxi as Codigo, login as AccountID, clave as Password, CONCAT(apellidos, ' ', nombres) as Description FROM taxi WHERE login = @Login AND clave = @Clave";
                    break;
                default:
                    throw new ArgumentException("Tipo de usuario no válido", nameof(tipo));
            }

            return await _defaultConnection.QueryFirstOrDefaultAsync<Account>(sql, new { Login = login, Clave = clave });
        }
    }
}
