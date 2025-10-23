using Microsoft.AspNetCore.Mvc;
using Services;
using ClosedXML.Excel;
using System.IO;

namespace FUNewsManagementSystem.Controllers
{
    public class ReportController : Controller
    {
        private readonly INewsService newsService;
        public ReportController(INewsService _newsService)
        {
            newsService = _newsService;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");
            // Lấy số liệu thống kê
            var countByCat = newsService.CountByCategory();
            var countByStatus = newsService.CountByStatus();
            var countByAuthor = newsService.CountByAuthor();
            ViewBag.CountByCategory = countByCat;
            ViewBag.CountByStatus = countByStatus;
            ViewBag.CountByAuthor = countByAuthor;
            return View();
        }

        public IActionResult ExportExcel()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login", "Account");
            // Lấy lại dữ liệu thống kê
            var countByCat = newsService.CountByCategory();
            var countByStatus = newsService.CountByStatus();
            var countByAuthor = newsService.CountByAuthor();
            // Tạo Workbook và các Worksheet
            using (var workbook = new XLWorkbook())
            {
                // Sheet 1: Thống kê theo Category
                var ws1 = workbook.AddWorksheet("ByCategory");
                ws1.Cell(1, 1).Value = "Danh mục";
                ws1.Cell(1, 2).Value = "Số bài viết";
                int row = 2;
                foreach (var entry in countByCat)
                {
                    ws1.Cell(row, 1).Value = entry.Key.CategoryName;
                    ws1.Cell(row, 2).Value = entry.Value;
                    row++;
                }
                ws1.Columns().AdjustToContents();

                // Sheet 2: Thống kê theo Status
                var ws2 = workbook.AddWorksheet("ByStatus");
                ws2.Cell(1, 1).Value = "Trạng thái";
                ws2.Cell(1, 2).Value = "Số bài viết";
                row = 2;
                foreach (var entry in countByStatus)
                {
                    ws2.Cell(row, 1).Value = entry.Key;
                    ws2.Cell(row, 2).Value = entry.Value;
                    row++;
                }
                ws2.Columns().AdjustToContents();

                // Sheet 3: Thống kê theo Tác giả
                var ws3 = workbook.AddWorksheet("ByAuthor");
                ws3.Cell(1, 1).Value = "Tác giả";
                ws3.Cell(1, 2).Value = "Số bài viết";
                row = 2;
                foreach (var entry in countByAuthor)
                {
                    ws3.Cell(row, 1).Value = entry.Key.AccountName + " (" + entry.Key.AccountEmail + ")";
                    ws3.Cell(row, 2).Value = entry.Value;
                    row++;
                }
                ws3.Columns().AdjustToContents();

                // Xuất workbook ra memory stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var fileName = $"Report_{System.DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    var content = stream.ToArray();
                    // Trả file Excel
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
            }
        }
    }
}
