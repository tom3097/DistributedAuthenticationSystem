using DistributedAuthSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAuthSystem.Services
{
    public interface INeighboursRepository
    {
        #region methods

        Neighbour[] GetAllNeighbours();

        Neighbour GetSingleNeighbour(int id);

        bool PostNeighbour(int id, string url);

        bool DeleteNeighbour(int id);

        bool SetSpecialNeighbour(int id, bool isSpecial);

        #endregion
    }
}
