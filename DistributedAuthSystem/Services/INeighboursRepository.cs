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

        Neighbour GetSingleNeighbour(string id);

        bool PostNeighbour(string id, string url);

        bool DeleteNeighbour(string id);

        bool SetSpecialNeighbour(string id, bool isSpecial);

        #endregion
    }
}
