﻿using DistributedAuthSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace DistributedAuthSystem.Services
{
    public class NeighboursRepository
    {
        #region fields

        private Dictionary<int, Neighbour> _repository;

        private readonly ReaderWriterLockSlim _lockSlim;

        #endregion

        #region methods

        public NeighboursRepository()
        {
            _repository = new Dictionary<int, Neighbour>();
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

        public Neighbour GetSingleNeighbour(int id)
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

        public bool PostNeighbour(Neighbour neighbour)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                if (!_repository.ContainsKey(neighbour.Id))
                {
                    _repository.Add(neighbour.Id, neighbour);
                    return true;
                }
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }

            return false;
        }

        public bool DeleteNeighbour(int id)
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

        public bool SetSpecialNeighbour(int id, bool isSpecial)
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