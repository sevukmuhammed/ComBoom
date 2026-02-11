namespace ComBoom.Social
{
    /// <summary>
    /// Leaderboard'da tek bir satırı temsil eden veri sınıfı
    /// </summary>
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public long Score { get; set; }
        public bool IsLocalPlayer { get; set; }

        public LeaderboardEntry() { }

        public LeaderboardEntry(int rank, string playerId, string playerName, long score, bool isLocalPlayer = false)
        {
            Rank = rank;
            PlayerId = playerId;
            PlayerName = playerName;
            Score = score;
            IsLocalPlayer = isLocalPlayer;
        }
    }
}
