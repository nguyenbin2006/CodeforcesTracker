using System.ComponentModel.DataAnnotations;
namespace CodeforcesTracker.Models
{
    public class Problem
    {
        [Key]
        public string ProblemId { get; set; }
        public string Name { get; set; }
        public int? DifficultyRating { get; set; }
    }
}
