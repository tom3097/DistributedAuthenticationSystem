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

        private Dictionary<string, Client> _repository;

        private readonly ReaderWriterLockSlim _lockSlim;

        private readonly JavaScriptSerializer _serializer;

        private readonly OperationsLog _operationsLog;

        #endregion

        #region methods

        public ClientsRepository()
        {
            _repository = new Dictionary<string, Client>();
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

        public Client GetSingleClient(string id)
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

        public bool PostClient(string id, string pin)
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

                    var clientAfter = client.DeepCopy();
                    _operationsLog.Add(OperationType.ADD_CLIENT, null, clientAfter);

                    return true;
                }
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }

            return false;
        }

        public bool DeleteClient(string id, string pin, out bool notFound)
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

                        var clientBefore = client.DeepCopy();
                        _operationsLog.Add(OperationType.DELETE_CLIENT, clientBefore, null);

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

        public bool ChangeClientPin(string id, string currentPin, string newPin, out bool notFound)
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
                        var clientBefore = client.DeepCopy();

                        client.Pin = newPin;

                        var clientAfter = client.DeepCopy();
                        _operationsLog.Add(OperationType.CHANGE_PASSWORD, clientBefore, clientAfter);

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

        public bool AuthenticateClient(string id, string pin, out bool notFound)
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

        public OneTimePasswordList GetClientPassList(string id, string pin, out bool notFound)
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

        public bool CheckCurrentPassword(string id, string pin, string oneTimePassword, out bool notFound)
        {
            _lockSlim.EnterReadLock();
            try
            {
                Client client;
                if (_repository.TryGetValue(id, out client))
                {
                    notFound = false;

                    if (client.Pin == pin && client.CurrentActivePassword() == oneTimePassword)
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

        public bool AuthorizeOperation(string id, string pin, string oneTimePassword, out bool notFound)
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
                        var clientBefore = client.DeepCopy();

                        client.UseCurrentActivePassword();

                        var clientAfter = client.DeepCopy();
                        _operationsLog.Add(OperationType.AUTHORIZATION, clientBefore, clientAfter);

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

        public bool ActivateNewPassList(string id, string pin, string oneTimePassword, out bool notFound)
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
                        var clientBefore = client.DeepCopy();

                        client.ActivateNewPassList();

                        var clientAfter = client.DeepCopy();
                        _operationsLog.Add(OperationType.LIST_ACTIVATION, clientBefore, clientAfter);

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
            var clientBefore = operation.DataBefore;
            var clientAfter = operation.DataAfter;
            string id = clientBefore == null ? clientAfter.Id : clientBefore.Id;
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
            var clientBefore = operation.DataBefore;
            var clientAfter = operation.DataAfter;
            string id = clientBefore == null ? clientAfter.Id : clientBefore.Id;
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

        public FatSynchroResult UpdateHistory(Operation[] operations, out long maxSynchroTime)
        {
            maxSynchroTime = -1;

            _lockSlim.EnterWriteLock();
            try
            {
                /* Checking if history log is empty */
                if (_operationsLog.IsEmpty())
                {
                    for (int i = 0; i < operations.Length; ++i)
                    {
                        UpdateMissingOperation(operations[i]);
                    }
                    _operationsLog.AddMissing(operations);

                    return FatSynchroResult.OK;
                }

                /* Checking if last hash in operations and _operationsLog's history
                 * are the same. If they are - servers are already synchronized */
                if (_operationsLog.GetLastHash() == operations.Last().Hash)
                {
                    return FatSynchroResult.ALREADY_SYNC;
                }

                /* Checking if the first log in operations can be a continuation
                 * of _operationsLog's history */
                var firstOpe = operations.First();
                Operation firstOpeBefore = _operationsLog.GetFirstOpeBefore(firstOpe.Timestamp);
                if (firstOpeBefore != null)
                {
                    var serialBefore = _serializer.Serialize(firstOpe.DataBefore);
                    var serialAfter = _serializer.Serialize(firstOpe.DataAfter);
                    string continueHash = OperationsLog.GenerateHash(firstOpe.Timestamp, firstOpe.Type,
                        serialBefore, serialAfter, firstOpeBefore.Hash);

                    if (continueHash != firstOpe.Hash)
                    {
                        return FatSynchroResult.CONFLICT;
                    }
                }

                /* Checking if the following operations are matching each other */
                var localOperations = _operationsLog.GetOperationsSince(firstOpe.Timestamp - 1);
                int minLength = operations.Length < localOperations.Length ? operations.Length : localOperations.Length;
                for (int i = 0; i < minLength; ++i)
                {
                    /* Not every operation matches */
                    if (operations[i].Hash != localOperations[i].Hash)
                    {
                        /* Checking if sender is older than the receiver */
                        if (operations[i].Timestamp > localOperations[i].Timestamp)
                        {
                            return FatSynchroResult.CONFLICT;
                        }

                        /* Receiver is older than sender */
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

                        return FatSynchroResult.FIXED;
                    }
                }

                /* All operations match and their number is equal */
                if (operations.Length == localOperations.Length)
                {
                    return FatSynchroResult.OK;
                }

                /* All operations match and there are more operations in receiver
                 * than in the sender */
                if (localOperations.Length > operations.Length)
                {
                    return FatSynchroResult.U2OLD;
                }

                /* All operations match and there are more operations in the sender
                 * than in the receiver */
                for (int i = minLength; i < operations.Length; ++i)
                {
                    UpdateMissingOperation(operations[i]);
                }
                var toAdd = new List<Operation>(operations).GetRange(minLength, operations.Length - minLength).ToArray();
                _operationsLog.AddMissing(toAdd);

                return FatSynchroResult.OK;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public long GetLastOperationTsmp()
        {
            return _operationsLog.GetLastTimestamp();
        }

        public string GetLastHash()
        {
            return _operationsLog.GetLastHash();
        }

        #endregion
    }
}