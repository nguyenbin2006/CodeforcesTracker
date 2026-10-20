using System.ComponentModel.DataAnnotations;
namespace CodeforcesTracker.Models
{
    public class User
    {
        [Key]
        public string Handle { get; set; }
        public int? CurrentRating { get; set; }
        public int? MaxRating { get; set; }
        public string? RankName { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
