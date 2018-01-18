using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Constants
{
    public enum SynchroResultType
    {
        OK,
        ALREADY_SYNC,
        CONFLICT,
        FIXED,
        U2OLD
    }
}