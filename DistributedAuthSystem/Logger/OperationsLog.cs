using DistributedAuthSystem.Constants;
using DistributedAuthSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;

namespace DistributedAuthSystem.Logger
{
    public class OperationsLog
    {
        #region fields

        private readonly List<Operation> _history;

        private readonly ReaderWriterLockSlim _lockSlim;

        private string _lastHash;

        #endregion

        #region methods

        public OperationsLog()
        {
            _history = new List<Operation>();
            _lockSlim = new ReaderWriterLockSlim();
            _lastHash = null;
        }

        public void Add(OperationType operationType, string serializedData)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                var operation = new Operation
                {
                    Timestamp = GenerateTimestamp(),
                    Hash = GenerateHash(operationType, serializedData),
                    SequenceNumber = _history.Count,
                    Type = operationType,
                    Data = serializedData
                };
                _history.Add(operation);
                _lastHash = operation.Hash;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        private static long GenerateTimestamp()
        {
            return Convert.ToInt64(DateTime.Now.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
        }

        private string GenerateHash(OperationType operationType, string serializedData)
        {
            var plainText = String.Format("{0}{1}{2}", operationType.ToString(), serializedData,
                _lastHash ?? "");
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            using (HashAlgorithm sha = new SHA256Managed())
            {
                byte[] encryptedBytes = sha.TransformFinalBlock(data, 0, data.Length);
                return Convert.ToBase64String(sha.Hash);
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

        #endregion
    }
}