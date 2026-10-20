namespace CodeforcesTracker.DTOs
{
    public class TagAnalysisResult
    {
        public string TagName { get; set; }
        public int TotalAttempts { get; set; } // Tổng số lần nộp bài có tag này
        public int FailedAttempts { get; set; } // Số lần nộp bị sai (Khác "OK")
        public double FailureRate { get; set; } // Tỷ lệ sai (%)
    }
}