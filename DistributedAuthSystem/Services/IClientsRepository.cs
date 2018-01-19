using DistributedAuthSystem.Constants;
using DistributedAuthSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedAuthSystem.Services
{
    public interface IClientsRepository
    {
        #region methods

        Client[] GetAllClients();

        Client GetSingleClient(string id);

        bool PostClient(string id, string pin);

        bool DeleteClient(string id, string pin, out bool notFound);

        bool ChangeClientPin(string id, string currentPin, string newPin, out bool notFound);

        bool AuthenticateClient(string id, string pin, out bool notFound);

        OneTimePasswordList GetClientPassList(string id, string pin, out bool notFound);

        bool CheckCurrentPassword(string id, string pin, string oneTimePassword, out bool notFound);

        bool AuthorizeOperation(string id, string pin, string oneTimePassword, out bool notFound);

        bool ActivateNewPassList(string id, string pin, string oneTimePassword, out bool notFound);

        Operation[] GetHistorySince(long timestamp);

        FatSynchroResult UpdateHistory(Operation[] operations, out long maxSynchroTime);

        long GetLastOperationTsmp();

        string GetLastHash();

        #endregion
    }
}
