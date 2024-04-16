using System;

namespace DefaultNamespace.Models
{
    [Serializable]
    public class LeaderboardRecord
    {
        public long Score { get; set; }
        public long Combo { get; set; }
        public long ItemsHit { get; set; }
        public long ItemsTotal { get; set; }
        public string MapKey { get; set; }
    }
}