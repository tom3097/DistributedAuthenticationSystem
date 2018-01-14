using DistributedAuthSystem.Constants;
using DistributedAuthSystem.Extensions;
using DistributedAuthSystem.Logger;
using DistributedAuthSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;

namespace DistributedAuthSystem.Services
{
    public class ClientsRepository : IClientsRepository
    {
        #region fields

        private Dictionary<int, Client> _repository;

        private readonly ReaderWriterLockSlim _lockSlim;

        private readonly JavaScriptSerializer _serializer;

        private readonly OperationsLog _operationsLog;

        #endregion

        #region methods

        public ClientsRepository()
        {
            _repository = new Dictionary<int, Client>();
            _lockSlim = new ReaderWriterLockSlim();
            _serializer = new JavaScriptSerializer();
            _operationsLog = new OperationsLog();
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

        public bool PostClient(int id, int pin)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                if (!_repository.ContainsKey(id))
                {
                    Client client = new Client
                    {
                        Id = id,
                        Pin = pin
                    };
                    client.InitializePasswordLists();
                    _repository.Add(id, client);

                    var serializedData = _serializer.Serialize(client);
                    _operationsLog.Add(OperationType.ADD_CLIENT, serializedData);

                    return true;
                }
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }

            return false;
        }

        public bool DeleteClient(int id, int pin, out bool notFound)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                Client client;
                if (_repository.TryGetValue(id, out client))
                {
                    notFound = false;

                    if (client.Pin == pin)
                    {
                        _repository.Remove(id);

                        var serializedData = _serializer.Serialize(client);
                        _operationsLog.Add(OperationType.DELETE_CLIENT, serializedData);

                        return true;
                    }

                    return false;
                }
                notFound = true;
                return false;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public bool ChangeClientPin(int id, int currentPin, int newPin, out bool notFound)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                Client client;
                if (_repository.TryGetValue(id, out client))
                {
                    notFound = false;

                    if (client.Pin == currentPin)
                    {
                        client.Pin = newPin;

                        var serializedData = _serializer.Serialize(client);
                        _operationsLog.Add(OperationType.CHANGE_PASSWORD, serializedData);

                        return true;
                    }

                    return false;
                }
                notFound = true;
                return false;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public bool AuthenticateClient(int id, int pin, out bool notFound)
        {
            _lockSlim.EnterReadLock();
            try
            {
                Client client;
                if (_repository.TryGetValue(id, out client))
                {
                    notFound = false;

                    if (client.Pin == pin)
                    {
                        return true;
                    }

                    return false;
                }
                notFound = true;
                return false;
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public OneTimePasswordList GetClientPassList(int id, int pin, out bool notFound)
        {
            _lockSlim.EnterReadLock();
            try
            {
                Client client;
                if (_repository.TryGetValue(id, out client))
                {
                    notFound = false;

                    if (client.Pin == pin)
                    {
                        return client.ActivatedList;
                    }

                    return null;
                }

                notFound = true;
                return null;
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public bool AuthorizeOperation(int id, int pin, string oneTimePassword, out bool notFound)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                Client client;
                if (_repository.TryGetValue(id, out client))
                {
                    notFound = false;

                    if (client.Pin == pin && client.CanAuthorizeOperation() &&
                        client.CurrentActivePassword() == oneTimePassword)
                    {
                        client.UseCurrentActivePassword();

                        var serializedData = _serializer.Serialize(client);
                        _operationsLog.Add(OperationType.AUTHORIZATION, serializedData);

                        return true;
                    }

                    return false;
                }

                notFound = true;
                return false;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public bool ActivateNewPassList(int id, int pin, string oneTimePassword, out bool notFound)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                Client client;
                if (_repository.TryGetValue(id, out client))
                {
                    notFound = false;

                    if (client.Pin == pin && client.CanActivateNewPassList() &&
                        client.CurrentActivePassword() == oneTimePassword)
                    {
                        client.ActivateNewPassList();

                        var serializedData = _serializer.Serialize(client);
                        _operationsLog.Add(OperationType.LIST_ACTIVATION, serializedData);

                        return true;
                    }

                    return false;
                }

                notFound = true;
                return false;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public Operation[] GetHistorySince(long timestamp)
        {
            _lockSlim.EnterReadLock();
            try
            {
                return _operationsLog.GetOperationsSince(timestamp);
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        private void UpdateMissingOperation(Operation operation)
        {
            // odzyskiwanie danych -> z json obiekt klasy klient
            var client = _serializer.Deserialize<Client>(operation.Data);
            switch (operation.Type)
            {
                case OperationType.ADD_CLIENT:
                    _repository.Add(client.Id, client);
                    break;
                case OperationType.AUTHORIZATION:
                    break;
                case OperationType.CHANGE_PASSWORD:
                    break;
                case OperationType.DELETE_CLIENT:
                    break;
                case OperationType.LIST_ACTIVATION:
                    break;
            }
        }

        public SynchroResultType UpdateHistory(Operation[] operations)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                if (operations == null || operations.Count() == 0)
                {
                    // chyba nie powinno sie zdarzyc ale co tam
                    return SynchroResultType.OK;
                }
                // sprawdzamy czy serwer ma juz aktualna historie
                if (_operationsLog.GetLastHash() == operations.Last().Hash)
                {
                    // operacja juz zsynchronizowana
                }

                // jedziemy z KOKOSEM -_-
                var operation = operations.First();
                var timeOfFirstOpe = operation.Timestamp;

                var opes = _operationsLog.GetOperationsSince(timeOfFirstOpe-1);

                /* porownywanie operacji nowych i lokalnych */
                int maxi = operations.Length < opes.Length ? operations.Length : opes.Length;

                /* sprawdzanie czy sa konflikty */
                for (int i = 0; i < maxi; ++i)
                {
                    if (operations[i].Hash != opes[i].Hash)
                    {
                        /* wykryto konflikt, jeszcze nie wiedomo kto ma racje */
                        return SynchroResultType.CONFLICT;
                    }
                }

                if (operations.Length == opes.Length)
                {
                    /* tyle samo operacji, ktore zostaly juz porownane, wiec wszystko ok */
                    /* already sync */
                    return SynchroResultType.OK;
                }

                /* brak konfliktow */
                if (opes.Length > operations.Length)
                {
                    /* my posiadamy wiecej informacji niz tamten serwer, powiedz mu ze jest nieaktualny */
                    return SynchroResultType.U2OLD;
                }

                /* tU: sender ma wiecej operacji niz my, przy czym operacje wspolne nie koliduja */
                int new_ope_idx = maxi;

                /* tu iterujemy po tych operacjach i jakos je dodajemy */
                for (int i = new_ope_idx; i < operations.Length; ++i)
                {
                    UpdateMissingOperation(operations[i]);
                }

                var toAdd = new List<Operation>(operations).GetRange(new_ope_idx, operations.Length - new_ope_idx).ToArray();
                _operationsLog.AddMissing(toAdd);

                return SynchroResultType.OK;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        #endregion
    }
}