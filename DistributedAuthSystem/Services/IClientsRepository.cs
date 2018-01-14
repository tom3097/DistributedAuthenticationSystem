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

        Client GetSingleClient(int id);

        bool PostClient(int id, int pin);

        bool DeleteClient(int id, int pin, out bool notFound);

        bool ChangeClientPin(int id, int currentPin, int newPin, out bool notFound);

        bool AuthenticateClient(int id, int pin, out bool notFound);

        OneTimePasswordList GetClientPassList(int id, int pin, out bool notFound);

        bool AuthorizeOperation(int id, int pin, string oneTimePassword, out bool notFound);

        bool ActivateNewPassList(int id, int pin, string oneTimePassword, out bool notFound);

        Operation[] GetHistorySince(long timestamp);

        #endregion
    }
}
