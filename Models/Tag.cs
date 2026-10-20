using System.ComponentModel.DataAnnotations;
namespace CodeforcesTracker.Models
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }
        public string TagName { get; set; }
    }
}
