namespace DistributedAuthSystem.Requests
{
    public class ChangeClientPinReq
    {
        #region properties

        public int CurrentPin { get; set; }

        public int NewPin { get; set; }

        #endregion
    }
}