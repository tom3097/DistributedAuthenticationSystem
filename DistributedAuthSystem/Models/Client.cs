namespace DistributedAuthSystem.Models
{
    public class Client
    {
        #region properties

        public int Id { get; set; }

        public int Pin { get; set; }

        public OneTimePasswordList ActivatedList { get; set; }

        public OneTimePasswordList NonactivatedList { get; set; }

        #endregion
    }
}