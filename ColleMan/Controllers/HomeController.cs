using ColleMan.Data;
using ColleMan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ColleMan.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DatabaseContext _databaseContext;
        public HomeController(ILogger<HomeController> logger, DatabaseContext databaseContext)
        {
            _logger = logger;
            _databaseContext = databaseContext;
        }

        public async Task<IActionResult> Index()
        {
            var recentItems = await _databaseContext.Items
            .OrderByDescending(i => i.DateCreated)
            .Take(5)
            .ToListAsync();

            var largestCollections = await _databaseContext.Collections
                .Include(c => c.Items)
                .OrderByDescending(c => c.Items.Count)
                .Take(5)
                .ToListAsync();

            var allTags = await _databaseContext.Tags
                .Select(t => t.Name)
                .Distinct()
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                RecentItems = recentItems,
                LargestCollections = largestCollections,
                AllTags = allTags
            };

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    
    }
}
