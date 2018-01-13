using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Models
{
    public class Neighbour
    {
        #region properties

        public int Id { get; set; }

        public string Url { get; set; }

        public bool IsSpecial { get; set; }

        #endregion
    }
}