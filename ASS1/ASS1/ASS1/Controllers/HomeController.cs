using System.Diagnostics;
using ASS1.Models;
using ASS1.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASS1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthenticationService _authService;

        public HomeController(ILogger<HomeController> logger, IAuthenticationService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            ViewBag.CurrentUser = currentUser;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
