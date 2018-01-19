using DistributedAuthSystem.Constants;

namespace DistributedAuthSystem.Models
{
    public class Operation
    {
        #region properties

        public long Timestamp { get; set; }

        public string Hash { get; set; }

        public int SequenceNumber { get; set; }

        public OperationType Type { get; set; }

        public string DataBefore { get; set; }

        public string DataAfter { get; set; }

        #endregion
    }
}