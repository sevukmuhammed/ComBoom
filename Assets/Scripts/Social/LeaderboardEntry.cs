namespace ComBoom.Social
{
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string PlayerName { get; set; }
        public long Score { get; set; }
        public bool IsLocalPlayer { get; set; }
    }
}
