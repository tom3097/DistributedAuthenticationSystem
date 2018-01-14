using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace DistributedAuthSystem.Services
{
    public class SynchroTimesRepository
    {
        #region fields

        private long _lastLocalModification;

        private Dictionary<int, long> _synchroMap;

        private readonly ReaderWriterLockSlim _modificationLockSlim;

        private readonly ReaderWriterLockSlim _mapLockSlim;

        #endregion

        #region properties

        public long LastLocalModification
        {
            get
            {
                _modificationLockSlim.EnterReadLock();
                try
                {
                    return _lastLocalModification;
                }
                finally
                {
                    _modificationLockSlim.ExitReadLock();
                }
            }
            set
            {
                _modificationLockSlim.EnterWriteLock();
                try
                {
                    _lastLocalModification = value;
                }
                finally
                {
                    _modificationLockSlim.ExitWriteLock();
                }
            }
        }

        #endregion

        #region methods

        public SynchroTimesRepository()
        {
            _synchroMap = new Dictionary<int, long>();
            _modificationLockSlim = new ReaderWriterLockSlim();
            _mapLockSlim = new ReaderWriterLockSlim();
        }

        public bool Add(int key, long value)
        {
            _mapLockSlim.EnterWriteLock();
            try
            {
                if (!_synchroMap.ContainsKey(key))
                {
                    _synchroMap.Add(key, value);
                    return true;
                }

                return false;
            }
            finally
            {
                _mapLockSlim.ExitWriteLock();
            }
        }

        public bool Update(int key, long value)
        {
            _mapLockSlim.EnterWriteLock();
            try
            {
                if(_synchroMap.ContainsKey(key))
                {
                    _synchroMap[key] = value;
                    return true;
                }

                return false;
            }
            finally
            {
                _mapLockSlim.ExitWriteLock();
            }
        }

        public bool Remove(int key)
        {
            _mapLockSlim.EnterWriteLock();
            try
            {
                return _synchroMap.Remove(key);
            }
            finally
            {
                _mapLockSlim.ExitReadLock();
            }
        }

        #endregion


    }
}