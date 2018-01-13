namespace DistributedAuthSystem.Models
{
    public class OneTimePasswordList
    {
        #region properties

        public string[] Passwords { get; set; }

        public int CurrentIndex { get; set; }

        #endregion
    }
}