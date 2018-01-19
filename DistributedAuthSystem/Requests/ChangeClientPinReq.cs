namespace DistributedAuthSystem.Requests
{
    public class ChangeClientPinReq
    {
        #region properties

        public string CurrentPin { get; set; }

        public string NewPin { get; set; }

        #endregion
    }
}