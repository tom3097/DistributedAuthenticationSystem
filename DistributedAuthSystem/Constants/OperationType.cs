using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Constants
{
    public enum OperationType
    {
        ADD_CLIENT,
        DELETE_CLIENT,
        CHANGE_PASSWORD,
        AUTHORIZATION,
        LIST_ACTIVATION
    }
}