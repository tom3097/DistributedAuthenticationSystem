using DistributedAuthSystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DistributedAuthSystem.Services
{
    public class NeighboursRepository : INeighboursRepository
    {
        #region fields

        private Dictionary<string, Neighbour> _repository;

        private readonly ReaderWriterLockSlim _lockSlim;

        #endregion

        #region methods

        public NeighboursRepository()
        {
            _repository = new Dictionary<string, Neighbour>();
            _lockSlim = new ReaderWriterLockSlim();
        }

        public Neighbour[] GetAllNeighbours()
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

        public Neighbour GetSingleNeighbour(string id)
        {
            Neighbour neighbour;
            bool success;

            _lockSlim.EnterReadLock();
            try
            {
                success = _repository.TryGetValue(id, out neighbour);
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }

            return success ? neighbour : null;
        }

        public bool PostNeighbour(string id, string url)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                if (!_repository.ContainsKey(id))
                {
                    Neighbour neighbour = new Neighbour
                    {
                        Id = id,
                        Url = url,
                        IsSpecial = false
                    };
                    _repository.Add(id, neighbour);
                    return true;
                }
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }

            return false;
        }

        public bool DeleteNeighbour(string id)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                return _repository.Remove(id);
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public bool SetSpecialNeighbour(string id, bool isSpecial)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                Neighbour neighbour;
                var success = _repository.TryGetValue(id, out neighbour);
                if (success)
                {
                    neighbour.IsSpecial = isSpecial;
                }
                return success;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        #endregion
    }
}