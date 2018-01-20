using DistributedAuthSystem.Constants;
using DistributedAuthSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace DistributedAuthSystem.Logger
{
    public class OperationsLog
    {
        #region fields

        private readonly List<Operation> _history;

        private readonly JavaScriptSerializer _serializer;

        private readonly ReaderWriterLockSlim _lockSlim;

        private string _lastHash;

        #endregion

        #region methods

        public OperationsLog()
        {
            _history = new List<Operation>();
            _serializer = new JavaScriptSerializer();
            _lockSlim = new ReaderWriterLockSlim();
            _lastHash = null;
        }

        public void Add(OperationType operationType, Client clientBefore, Client clientAfter)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                var serialBefore = _serializer.Serialize(clientBefore);
                var serialAfter = _serializer.Serialize(clientAfter);

                var timestamp = GenerateTimestamp();

                var operation = new Operation
                {
                    Timestamp = timestamp,
                    Hash = GenerateHash(timestamp, operationType, serialBefore, serialAfter, _lastHash),
                    SequenceNumber = _history.Count,
                    Type = operationType,
                    DataBefore = clientBefore,
                    DataAfter = clientAfter
                };
                _history.Add(operation);
                _lastHash = operation.Hash;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public static long GenerateTimestamp()
        {
            return Convert.ToInt64(DateTime.Now.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
        }

        public static string GenerateHash(long timestamp, OperationType operationType, string serialBefore,
            string serialAfter, string lastHash)
        {
            var plainText = String.Format("{0}{1}{2}{3}{4}", timestamp.ToString(), operationType.ToString(),
                serialBefore, serialAfter, lastHash ?? "");

            byte[] data = Encoding.UTF8.GetBytes(plainText);

            using (HashAlgorithm sha = new SHA256Managed())
            {
                sha.ComputeHash(data);
                string hex = BitConverter.ToString(sha.Hash).Replace("-", string.Empty);

                return hex.ToLower();
            }
        }

        public Operation[] GetOperationsSince(long timestamp)
        {
            _lockSlim.EnterReadLock();
            try
            {
                var operations = new Stack<Operation>();

                int index = _history.Count - 1;
                while (index >= 0 && _history[index].Timestamp > timestamp)
                {
                    operations.Push(_history[index--]);
                }

                return operations.ToArray();
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public Operation GetFirstOpeBefore(long timestamp)
        {
            _lockSlim.EnterReadLock();
            try
            {
                int index = _history.Count - 1;
                while (index >= 0 && _history[index].Timestamp >= timestamp)
                {
                    --index;
                }

                return index == -1 ? null : _history[index];
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public string GetLastHash()
        {
            _lockSlim.EnterReadLock();
            try
            {
                return _lastHash;
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public long GetLastTimestamp()
        {
            _lockSlim.EnterReadLock();
            try
            {
                return _history.Count != 0 ? _history.Last().Timestamp : 0;
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public void AddMissing(Operation[] operations)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                _history.AddRange(operations);
                _lastHash = _history.Last().Hash;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public bool IsEmpty()
        {
            _lockSlim.EnterReadLock();
            try
            {
                return _history.Count == 0;
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public void RemoveFromTop(int counter)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                int index = _history.Count - counter;
                _history.RemoveRange(index, counter);
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        #endregion
    }
}