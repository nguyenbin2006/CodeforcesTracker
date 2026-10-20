using CodeforcesTracker.DTOs;
using CodeforcesTracker.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CodeforcesTracker.Services
{
    public class CodeforcesApiService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        // Constructor: Nạp DbContext và HttpClient vào
        public CodeforcesApiService(AppDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<string> FetchAndSaveSubmissionsAsync(string handle)
        {
            string apiUrl = $"https://codeforces.com/api/user.status?handle={handle}&from=1&count=100"; // Tạm lấy 100 bài để test

            try
            {
                // 1. Gọi API
                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return "Không thể kết nối đến API Codeforces.";
                }

                // 2. Đọc và Parse JSON
                var jsonString = await response.Content.ReadAsStringAsync();
                var apiData = JsonConvert.DeserializeObject<CodeforcesResponse<List<CFSubmission>>>(jsonString);

                if (apiData.Status != "OK" || apiData.Result == null)
                {
                    return "Lỗi từ Codeforces: Handle không tồn tại hoặc API lỗi.";
                }

                // 3. Đảm bảo User (Handle) đã tồn tại trong DB
                var user = await _context.Users.FindAsync(handle);
                if (user == null)
                {
                    user = new User { Handle = handle, LastUpdated = DateTime.Now };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                int newSubmissionsCount = 0;

                // 4. Xử lý từng bài nộp (Submission)
                foreach (var cfSub in apiData.Result)
                {
                    // Tránh lưu trùng lặp: Nếu submission này đã có trong DB thì bỏ qua
                    if (await _context.Submissions.AnyAsync(s => s.SubmissionId == cfSub.Id))
                        continue;

                    string problemDbId = $"{cfSub.Problem.ContestId}{cfSub.Problem.Index}"; // VD: 1900A

                    // 4.1. Lưu Bài toán (Problem) nếu chưa có
                    var problem = await _context.Problems.FindAsync(problemDbId);
                    if (problem == null)
                    {
                        problem = new Problem
                        {
                            ProblemId = problemDbId,
                            Name = cfSub.Problem.Name,
                            DifficultyRating = cfSub.Problem.Rating
                        };
                        _context.Problems.Add(problem);
                        await _context.SaveChangesAsync();

                        // 4.2. Lưu Tags của bài toán đó (dp, math, graphs...)
                        if (cfSub.Problem.Tags != null)
                        {
                            foreach (var tagName in cfSub.Problem.Tags)
                            {
                                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.TagName == tagName);
                                if (tag == null)
                                {
                                    tag = new Tag { TagName = tagName };
                                    _context.Tags.Add(tag);
                                    await _context.SaveChangesAsync(); // Cần lưu tag ngay để lấy Id
                                }

                                _context.ProblemTags.Add(new ProblemTag { ProblemId = problemDbId, TagId = tag.TagId });
                            }
                        }
                    }

                    // 4.3. Lưu Lịch sử nộp bài (Submission)
                    var submission = new Submission
                    {
                        SubmissionId = cfSub.Id,
                        Handle = handle,
                        ProblemId = problemDbId,
                        Verdict = cfSub.Verdict,
                        ProgrammingLanguage = cfSub.ProgrammingLanguage,
                        // Đổi từ Unix timestamp sang DateTime
                        CreationTime = DateTimeOffset.FromUnixTimeSeconds(cfSub.CreationTimeSeconds).DateTime
                    };

                    _context.Submissions.Add(submission);
                    newSubmissionsCount++;
                }

                // 5. Lưu toàn bộ thay đổi vào SQL Server
                await _context.SaveChangesAsync();
                return $"Thành công! Đã thêm {newSubmissionsCount} bài nộp mới vào Database.";
            }
            catch (Exception ex)
            {
                return $"Lỗi hệ thống: {ex.InnerException?.Message ?? ex.Message}";
            }
        }

        public async Task<List<TagAnalysisResult>> AnalyzeWeaknessesAsync(string handle)
        {
            // 1. Dùng LINQ Query để nối bảng và gom nhóm (Group by) theo TagName
            var rawData = await (from s in _context.Submissions
                                 where s.Handle == handle
                                 join pt in _context.ProblemTags on s.ProblemId equals pt.ProblemId
                                 join t in _context.Tags on pt.TagId equals t.TagId
                                 group s.Verdict by t.TagName into g
                                 select new
                                 {
                                     TagName = g.Key,
                                     TotalAttempts = g.Count(),
                                     // Đếm số lần verdict khác OK (tức là WA, TLE, RE...)
                                     FailedAttempts = g.Count(v => v != "OK")
                                 }).ToListAsync();

            // 2. Tính tỷ lệ % sai và sắp xếp tìm ra "Tử huyệt"
            var analysis = rawData
                .Select(x => new TagAnalysisResult
                {
                    TagName = x.TagName,
                    TotalAttempts = x.TotalAttempts,
                    FailedAttempts = x.FailedAttempts,
                    // Tính % sai. Nếu chưa làm bài nào thì = 0
                    FailureRate = x.TotalAttempts > 0
                        ? Math.Round((double)x.FailedAttempts / x.TotalAttempts * 100, 2)
                        : 0
                })
                // Lọc nhiễu: Chỉ xét các tag đã làm từ 3 lần trở lên
                .Where(x => x.TotalAttempts >= 3)
                // Sắp xếp: Tag nào sai nhiều nhất (tỷ lệ cao nhất) lên đầu
                .OrderByDescending(x => x.FailureRate)
                .ThenByDescending(x => x.FailedAttempts)
                .Take(10) // Lấy Top 10 điểm yếu lớn nhất
                .ToList();

            return analysis;
        }

        public async Task<List<CFProblem>> GetSuggestedProblemsAsync(string handle, string targetTag)
        {
            // 1. Lấy danh sách ID các bài toán bạn đã giải ĐÚNG (Verdict == "OK") từ SQL Server
            var solvedProblemIds = await _context.Submissions
                .Where(s => s.Handle == handle && s.Verdict == "OK")
                .Select(s => s.ProblemId)
                .Distinct()
                .ToListAsync();

            // 2. Gọi API lấy tất cả bài tập thuộc Tag điểm yếu của bạn
            string apiUrl = $"https://codeforces.com/api/problemset.problems?tags={targetTag}";

            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode) return new List<CFProblem>();

                var jsonString = await response.Content.ReadAsStringAsync();

                // Dùng DTO mới để ép kiểu JSON
                var apiData = JsonConvert.DeserializeObject<CodeforcesResponse<CFProblemsetResult>>(jsonString);

                if (apiData?.Status != "OK" || apiData.Result?.Problems == null) return new List<CFProblem>();

                // 3. Lọc bài và Gợi ý
                var suggestions = apiData.Result.Problems
                    .Where(p => p.Rating != null) // Chỉ lấy bài có Rating độ khó rõ ràng
                    .Where(p => !solvedProblemIds.Contains($"{p.ContestId}{p.Index}")) // BỎ QUA những bài đã giải đúng
                    .OrderBy(p => p.Rating) // Sắp xếp từ dễ đến khó
                    .Take(5) // Lấy 5 bài đầu tiên
                    .ToList();

                return suggestions;
            }
            catch
            {
                return new List<CFProblem>();
            }
        }
    }
}