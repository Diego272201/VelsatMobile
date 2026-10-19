using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Data.Services;

namespace VelsatMobile.Data.Repositories
{
    public interface IReadOnlyUnitOfWork : IDisposable
    {
        IDatosCargainicialService DatosCargainicialService { get; }

        IServidorRepository ServidorRepository { get; }

        IHistoricosRepository HistoricosRepository { get; }

        IKilometrosRepository KilometrosRepository { get; }

        IUserRepository UserRepository { get; }

        IAplicativoRepository AplicativoRepository { get; }
    }
}
