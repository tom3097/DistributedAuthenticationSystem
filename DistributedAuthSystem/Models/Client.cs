namespace DistributedAuthSystem.Models
{
    public class Client
    {
        #region properties

        public string Id { get; set; }

        public string Pin { get; set; }

        public OneTimePasswordList ActivatedList { get; set; }

        public OneTimePasswordList NonactivatedList { get; set; }

        #endregion
    }
}