namespace CodeforcesTracker.DTOs
{
    // Cấu trúc JSON chuẩn của Codeforces
    public class CodeforcesResponse<T>
    {
        public string Status { get; set; }
        public T Result { get; set; }
    }

    public class CFSubmission
    {
        public int Id { get; set; }
        public long CreationTimeSeconds { get; set; }
        public string ProgrammingLanguage { get; set; }
        public string Verdict { get; set; }
        public CFProblem Problem { get; set; }
        public CFAuthor Author { get; set; }
    }

    public class CFProblem
    {
        public int ContestId { get; set; }
        public string Index { get; set; } // Ví dụ: A, B, C
        public string Name { get; set; }
        public int? Rating { get; set; } // Có thể null nếu bài chưa có rating
        public List<string> Tags { get; set; }
    }

    public class CFAuthor
    {
        public List<CFMember> Members { get; set; }
    }

    public class CFMember
    {
        public string Handle { get; set; }
    }
    // Class hứng dữ liệu danh sách bài tập gợi ý
    public class CFProblemsetResult
    {
        public List<CFProblem> Problems { get; set; }
    }
}