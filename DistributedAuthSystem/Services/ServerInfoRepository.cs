using DistributedAuthSystem.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace DistributedAuthSystem.Services
{
    public class ServerInfoRepository : IServerInfoRepository
    {
        #region fields

        private string _id;

        private ServerState _state;

        private long _lastFatSynchro;

        private readonly ReaderWriterLockSlim _lockSlim;

        private readonly ReaderWriterLockSlim _lockSlimState;

        private readonly ReaderWriterLockSlim _lockSlimFatSynchro;

        #endregion

        #region methods

        public ServerInfoRepository()
        {
            _id = null;
            _state = ServerState.IS_OK;
            _lastFatSynchro = 0;
            _lockSlim = new ReaderWriterLockSlim();
            _lockSlimState = new ReaderWriterLockSlim();
            _lockSlimFatSynchro = new ReaderWriterLockSlim();
        }

        public string GetServerId()
        {
            _lockSlim.EnterReadLock();
            try
            {
                return _id;
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public bool PutServerId(string id)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                if (String.IsNullOrEmpty(_id))
                {
                    _id = id;
                    return true;
                }

                return false;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public ServerState GetServerState()
        {
            _lockSlimState.EnterReadLock();
            try
            {
                return _state;
            }
            finally
            {
                _lockSlimState.ExitReadLock();
            }
        }

        public void SetServerState(ServerState state)
        {
            _lockSlimState.EnterWriteLock();
            try
            {
                _state = state;
            }
            finally
            {
                _lockSlimState.ExitWriteLock();
            }
        }

        public long GetLastFatSynchro()
        {
            _lockSlimFatSynchro.EnterReadLock();
            try
            {
                return _lastFatSynchro;
            }
            finally
            {
                _lockSlimFatSynchro.ExitReadLock();
            }
        }

        public void SetLastFatSynchro(long lastFatSynchro)
        {
            _lockSlimFatSynchro.EnterWriteLock();
            try
            {
                _lastFatSynchro = lastFatSynchro;
            }
            finally
            {
                _lockSlimFatSynchro.ExitWriteLock();
            }
        }

        #endregion
    }
}