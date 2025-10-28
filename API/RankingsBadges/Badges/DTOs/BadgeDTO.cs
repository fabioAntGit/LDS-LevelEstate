namespace API.RankingsBadges.Badges.DTOs
{
    public class BadgeDTO
    {
        public int BadgeId { get; set; }
        public byte Position { get; set; }
        public byte? Month { get; set; }   
        public short Year { get; set; }    
    }
}
