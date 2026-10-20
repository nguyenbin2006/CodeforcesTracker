using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CodeforcesTracker.Models
{
    public class Submission
    {
        [Key]
        public int SubmissionId { get; set; }
        public string Handle { get; set; }
        public string ProblemId { get; set; }
        public string Verdict { get; set; }
        public string ProgrammingLanguage { get; set; }
        public DateTime? CreationTime { get; set; }
    }
}
