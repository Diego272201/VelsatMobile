using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelsatBackendAPI.Data.Services;
using VelsatMobile.Data.Repositories;

namespace VelsatBackendAPI.Data.Repositories
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }

        IDatosCargainicialService DatosCargainicialService { get; }

        IHistoricosRepository HistoricosRepository { get; }

        IKilometrosRepository KilometrosRepository { get; }

        IServidorRepository ServidorRepository { get; }

        IAplicativoRepository AplicativoRepository { get; }
    }
}
