namespace DistributedAuthSystem.Requests
{
    public class AuthPasswordReq
    {
        #region properties

        public string Pin { get; set; }

        public string OneTimePassword { get; set; }

        #endregion
    }
}