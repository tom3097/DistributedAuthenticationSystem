using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace DistributedAuthSystem.Services
{
    public class SynchronizationsRepository : ISynchronizationsRepository
    {
        #region fields

        private Dictionary<int, long> _synchroTimes;

        private readonly ReaderWriterLockSlim _lockSlim;

        #endregion

        #region methods

        public SynchronizationsRepository()
        {
            _synchroTimes = new Dictionary<int, long>();
            _lockSlim = new ReaderWriterLockSlim();
        }

        public bool RegisterServer(int id)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                if (!_synchroTimes.ContainsKey(id))
                {
                    _synchroTimes.Add(id, 0);
                    return true;
                }

                return false;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public bool UnregisterServer(int id)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                return _synchroTimes.Remove(id);
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public bool UpdateSynchroTime(int id, long timestamp)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                if(_synchroTimes.ContainsKey(id))
                {
                    _synchroTimes[id] = timestamp;
                    return true;
                }

                return false;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public bool GetSynchroTimesCopy(out Dictionary<int, long> synchroTimesCopy)
        {
            _lockSlim.EnterReadLock();
            try
            {
                synchroTimesCopy = new Dictionary<int, long>(_synchroTimes);
                return true;
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        #endregion
    }
}