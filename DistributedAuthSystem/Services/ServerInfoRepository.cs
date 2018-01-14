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

        private readonly ReaderWriterLockSlim _lockSlim;

        #endregion

        #region methods

        public ServerInfoRepository()
        {
            _id = null;
            _lockSlim = new ReaderWriterLockSlim();
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

        #endregion
    }
}