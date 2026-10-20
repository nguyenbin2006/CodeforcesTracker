using Microsoft.AspNetCore.Mvc;
using CodeforcesTracker.Services;

namespace CodeforcesTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly CodeforcesApiService _cfService;

        // Nạp CodeforcesApiService vào Controller
        public HomeController(CodeforcesApiService cfService)
        {
            _cfService = cfService;
        }

        // Giao diện hiển thị trang chủ (GET)
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Xử lý khi người dùng bấm nút Submit (POST)
        [HttpPost]
        public async Task<IActionResult> SyncData(string handle)
        {
            if (string.IsNullOrEmpty(handle))
            {
                TempData["Message"] = "Vui lòng nhập Handle Codeforces!";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }

            // Gọi Service để kéo dữ liệu
            var result = await _cfService.FetchAndSaveSubmissionsAsync(handle);

            // Gán thông báo để hiển thị (bất kể thành công hay thất bại)
            TempData["Message"] = result;

            // Kiểm tra xem đồng bộ có thành công không
            if (result.Contains("Thành công"))
            {
                TempData["MessageType"] = "success";

                // CHUYỂN HƯỚNG TỰ ĐỘNG SANG TRANG ANALYZE
                // new { handle = handle } sẽ tự động ghép thành URL: /Home/Analyze?handle=...
                return RedirectToAction("Analyze", new { handle = handle });
            }
            else
            {
                // Nếu API lỗi (Handle sai, mất mạng...), giữ lại ở trang Index để báo màu Vàng/Đỏ
                TempData["MessageType"] = "warning";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Analyze(string handle)
        {
            if (string.IsNullOrEmpty(handle)) return RedirectToAction("Index");

            // 1. Phân tích điểm yếu
            var weaknesses = await _cfService.AnalyzeWeaknessesAsync(handle);
            ViewBag.Handle = handle;

            // 2. Tự động gợi ý bài cho Tag yếu nhất (Top 1)
            if (weaknesses.Any())
            {
                var weakestTag = weaknesses.First().TagName;
                var suggestions = await _cfService.GetSuggestedProblemsAsync(handle, weakestTag);

                // Truyền dữ liệu gợi ý ra View
                ViewBag.WeakestTag = weakestTag;
                ViewBag.Suggestions = suggestions;
            }

            return View(weaknesses);
        }
    }
}