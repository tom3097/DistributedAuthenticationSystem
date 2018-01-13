using DistributedAuthSystem.Models;

namespace DistributedAuthSystem.Extensions
{
    public static class ClientExt
    {
        #region methods

        public static void InitializePasswordLists(this Client client)
        {
            client.ActivatedList = new OneTimePasswordList();
            client.NonactivatedList = new OneTimePasswordList();
            client.ActivatedList.GeneratePasswords();
            client.NonactivatedList.GeneratePasswords();
        }

        public static bool CanAuthorizeOperation(this Client client)
        {
            return client.ActivatedList.CanAuthorizeOperation();
        }

        public static bool CanActivateNewPassList(this Client client)
        {
            return client.ActivatedList.CanActivateNewPassList();
        }

        public static string CurrentActivePassword(this Client client)
        {
            return client.ActivatedList.CurrentPassword();
        }

        public static void UseCurrentActivePassword(this Client client)
        {
            client.ActivatedList.UseCurrentPassword();
        }

        public static void ActivateNewPassList(this Client client)
        {
            client.ActivatedList = client.NonactivatedList;
            client.NonactivatedList = new OneTimePasswordList();
            client.NonactivatedList.GeneratePasswords();
        }

        #endregion
    }
}