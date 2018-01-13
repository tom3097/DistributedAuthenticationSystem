using DistributedAuthSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace DistributedAuthSystem.Services
{
    public class ClientsRepository
    {
        #region fields

        private Dictionary<int, Client> _repository;

        private readonly ReaderWriterLockSlim _lockSlim;

        #endregion

        #region methods

        public ClientsRepository()
        {
            _repository = new Dictionary<int, Client>();
            _lockSlim = new ReaderWriterLockSlim();
        }

        public Client[] GetAllClients()
        {
            _lockSlim.EnterReadLock();
            try
            {
                return _repository.Values.ToArray();
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public Client GetSingleClient(int id)
        {
            Client client;
            bool success;

            _lockSlim.EnterReadLock();
            try
            {
                success = _repository.TryGetValue(id, out client);
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }

            return success ? client : null;
        }

        #endregion
    }
}