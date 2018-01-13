namespace DistributedAuthSystem.Requests
{
    public class AuthorizaOperationReq
    {
        #region properties

        public int Pin { get; set; }

        public string OneTimePassword { get; set; }

        #endregion
    }
}