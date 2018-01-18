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

                    var serializedAfter = _serializer.Serialize(client);
                    _operationsLog.Add(OperationType.ADD_CLIENT, null, serializedAfter);

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

                        var serializedBefore = _serializer.Serialize(client);
                        _operationsLog.Add(OperationType.DELETE_CLIENT, serializedBefore, null);

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
                        var serializedBefore = _serializer.Serialize(client);

                        client.Pin = newPin;

                        var serializedAfter = _serializer.Serialize(client);
                        _operationsLog.Add(OperationType.CHANGE_PASSWORD, serializedBefore, serializedAfter);

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
                        var serializedBefore = _serializer.Serialize(client);

                        client.UseCurrentActivePassword();

                        var serializedAfter = _serializer.Serialize(client);
                        _operationsLog.Add(OperationType.AUTHORIZATION, serializedBefore, serializedAfter);

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
                        var serializedBefore = _serializer.Serialize(client);

                        client.ActivateNewPassList();

                        var serializedAfter = _serializer.Serialize(client);
                        _operationsLog.Add(OperationType.LIST_ACTIVATION, serializedBefore, serializedAfter);

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
            var clientBefore = operation.DataBefore == null ? null :
                _serializer.Deserialize<Client>(operation.DataBefore);
            var clientAfter = operation.DataAfter == null ? null :
                _serializer.Deserialize<Client>(operation.DataAfter);
            int id = clientBefore == null ? clientAfter.Id : clientBefore.Id;
            switch (operation.Type)
            {
                case OperationType.ADD_CLIENT:
                    _repository.Add(id, clientAfter);
                    break;
                case OperationType.AUTHORIZATION:
                    _repository[id] = clientAfter;
                    break;
                case OperationType.CHANGE_PASSWORD:
                    _repository[id] = clientAfter;
                    break;
                case OperationType.DELETE_CLIENT:
                    _repository.Remove(id);
                    break;
                case OperationType.LIST_ACTIVATION:
                    _repository[id] = clientAfter;
                    break;
            }
        }

        private void UndoOperation(Operation operation)
        {
            var clientBefore = operation.DataBefore == null ? null :
                _serializer.Deserialize<Client>(operation.DataBefore);
            var clientAfter = operation.DataAfter == null ? null :
                _serializer.Deserialize<Client>(operation.DataAfter);
            int id = clientBefore == null ? clientAfter.Id : clientBefore.Id;
            switch (operation.Type)
            {
                case OperationType.ADD_CLIENT:
                    _repository.Remove(id);
                    break;
                case OperationType.AUTHORIZATION:
                    _repository[id] = clientBefore;
                    break;
                case OperationType.CHANGE_PASSWORD:
                    _repository[id] = clientBefore;
                    break;
                case OperationType.DELETE_CLIENT:
                    _repository[id] = clientBefore;
                    break;
                case OperationType.LIST_ACTIVATION:
                    _repository[id] = clientBefore;
                    break;
            }
        }

        public SynchroResultType UpdateHistory(Operation[] operations, out long maxSynchroTime)
        {
            maxSynchroTime = -1;

            _lockSlim.EnterWriteLock();
            try
            {
                if (_operationsLog.GetLastHash() == operations.Last().Hash)
                {
                    return SynchroResultType.ALREADY_SYNC;
                }

                var firstOpeTimestamp = operations.First().Timestamp;
                var localOperations = _operationsLog.GetOperationsSince(firstOpeTimestamp - 1);

                int minLength = operations.Length < localOperations.Length ? operations.Length : localOperations.Length;
                for (int i = 0; i < minLength; ++i)
                {
                    if (operations[i].Hash != localOperations[i].Hash)
                    {
                        if (operations[i].Timestamp < localOperations[i].Timestamp)
                        {
                            for (int j = i; j < localOperations.Length; ++j)
                            {
                                UndoOperation(localOperations[j]);
                            }
                            int counter = localOperations.Length - i;
                            _operationsLog.RemoveFromTop(counter);

                            for (int j = i; j < operations.Length; ++j)
                            {
                                UpdateMissingOperation(operations[j]);
                            }
                            var missingConf = new List<Operation>(operations).GetRange(i, operations.Length - i).ToArray();
                            _operationsLog.AddMissing(missingConf);

                            maxSynchroTime = i > 0 ? operations[i - 1].Timestamp : 0;

                            return SynchroResultType.FIXED;
                        }

                        return SynchroResultType.CONFLICT;
                    }
                }

                if (operations.Length == localOperations.Length)
                {
                    return SynchroResultType.OK;
                }

                if (localOperations.Length > operations.Length)
                {
                    return SynchroResultType.U2OLD;
                }

                for (int i = minLength; i < operations.Length; ++i)
                {
                    UpdateMissingOperation(operations[i]);
                }
                var toAdd = new List<Operation>(operations).GetRange(minLength, operations.Length - minLength).ToArray();
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