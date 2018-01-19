using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DistributedAuthSystem.Services
{
    public class SynchronizationsRepository : ISynchronizationsRepository
    {
        #region fields

        private Dictionary<string, long> _synchroTimes;

        private readonly ReaderWriterLockSlim _lockSlim;

        #endregion

        #region methods

        public SynchronizationsRepository()
        {
            _synchroTimes = new Dictionary<string, long>();
            _lockSlim = new ReaderWriterLockSlim();
        }

        public bool RegisterServer(string id)
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

        public bool UnregisterServer(string id)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                return _synchroTimes.Remove(id);
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public bool UpdateTime(string id, long timestamp)
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

        public bool GetSynchroTimesCopy(out Dictionary<string, long> synchroTimesCopy)
        {
            _lockSlim.EnterReadLock();
            try
            {
                synchroTimesCopy = new Dictionary<string, long>(_synchroTimes);
                return true;
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public void UpdateSynchroTimes(Dictionary<string, long> synchroTimesSource, string sourceId,
            long synchroToTime)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                foreach (var key in _synchroTimes.Keys.ToList())
                {
                    if (key == sourceId || !synchroTimesSource.ContainsKey(key))
                    {
                        continue;
                    }

                    if (_synchroTimes[key] < synchroTimesSource[key])
                    {
                        _synchroTimes[key] = synchroTimesSource[key] > synchroToTime
                            ? synchroToTime : synchroTimesSource[key];
                    }
                }
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public void FixSynchroTimes(long maxSynchroTime)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                foreach (var key in _synchroTimes.Keys.ToList())
                {
                    if (_synchroTimes[key] > maxSynchroTime)
                    {
                        _synchroTimes[key] = maxSynchroTime;
                    }
                }
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        #endregion
    }
}