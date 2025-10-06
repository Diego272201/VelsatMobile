using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Data.Services;
using VelsatMobile.Data.Repositories;

namespace VelsatBackendAPI.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly IDbConnection _defaultConnection;
        private IDbTransaction _defaultTransaction;

        private readonly IDbConnection _secondConnection;
        private IDbTransaction _secondTransaction;

        private readonly IUserRepository _userRepository;
        private readonly IDatosCargainicialService _datosCargaInicialService;
        private readonly IHistoricosRepository _historicosRepository;
        private readonly IKilometrosRepository _kilometrosRepository;
        private readonly IServidorRepository _servidorRepository;
        private readonly IAplicativoRepository _aplicativoRepository;

        private bool _disposed = false;

        public UnitOfWork(MySqlConfiguration configuration, IConfiguration config)
        {
            _defaultConnection = new MySqlConnection(configuration.DefaultConnection);
            _defaultConnection.Open();
            _defaultTransaction = _defaultConnection.BeginTransaction();

            _secondConnection = new MySqlConnection(configuration.SecondConnection);
            _secondConnection.Open();
            _secondTransaction = _secondConnection.BeginTransaction();

            _userRepository = new UserRepository(_defaultConnection, _defaultTransaction);
            _datosCargaInicialService = new DatosCargainicialService(_defaultConnection, _defaultTransaction);
            _historicosRepository = new HistoricosRepository(_defaultConnection, _secondConnection, _secondTransaction);
            _kilometrosRepository = new KilometrosRepository(_defaultConnection, _secondConnection, _defaultTransaction, _secondTransaction);

            _servidorRepository = new ServidorRepository(_defaultConnection);

            _aplicativoRepository = new AplicativoRepository(_defaultConnection, _defaultTransaction);
        }

        public IUserRepository UserRepository => _userRepository;
        public IDatosCargainicialService DatosCargainicialService => _datosCargaInicialService;
        public IHistoricosRepository HistoricosRepository => _historicosRepository;
        public IKilometrosRepository KilometrosRepository => _kilometrosRepository;
        public IServidorRepository ServidorRepository => _servidorRepository;
        public IAplicativoRepository AplicativoRepository => _aplicativoRepository;

        public void SaveChanges()
        {
            try
            {
                _defaultTransaction?.Commit();
                _secondTransaction?.Commit();
            }
            catch
            {
                // Si hay error, hacer rollback
                _defaultTransaction?.Rollback();
                _secondTransaction?.Rollback();
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    // Primero hacer rollback si las transacciones siguen activas
                    if (_defaultTransaction != null)
                    {
                        _defaultTransaction.Rollback();
                        _defaultTransaction.Dispose();
                        _defaultTransaction = null;
                    }

                    if (_secondTransaction != null)
                    {
                        _secondTransaction.Rollback();
                        _secondTransaction.Dispose();
                        _secondTransaction = null;
                    }

                    // Luego cerrar y liberar las conexiones
                    if (_defaultConnection != null)
                    {
                        if (_defaultConnection.State == ConnectionState.Open)
                        {
                            _defaultConnection.Close();
                        }
                        _defaultConnection.Dispose();
                    }

                    if (_secondConnection != null)
                    {
                        if (_secondConnection.State == ConnectionState.Open)
                        {
                            _secondConnection.Close();
                        }
                        _secondConnection.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing UnitOfWork: {ex.Message}");
                }
                finally
                {
                    _disposed = true;
                }
            }
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}