using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Constants
{
    public enum FatSynchroResult
    {
        OK,
        ALREADY_SYNC,
        CONFLICT,
        FIXED,
        U2OLD
    }
}