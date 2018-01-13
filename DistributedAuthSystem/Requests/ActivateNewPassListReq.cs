namespace DistributedAuthSystem.Requests
{
    public class ActivateNewPassListReq
    {
        #region properties

        public int Pin { get; set; }

        public string OneTimePassword { get; set; }

        #endregion
    }
}