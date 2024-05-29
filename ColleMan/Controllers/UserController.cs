using ColleMan.Data;
using ColleMan.Interfaces;
using ColleMan.Migrations;
using ColleMan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ColleMan.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DatabaseContext _dbContext;
        private readonly ICloud _cloud;
        public UserController(UserManager<ApplicationUser> userManager,
            DatabaseContext databaseContext,
            ICloud cloud)
        {
            _userManager = userManager;
            _dbContext = databaseContext;
            _cloud = cloud;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> MyCollections()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return NotFound();
            var userCollections = _dbContext.Collections
            .Where(c => c.User.Id == currentUser.Id)
            .ToList();

            return View(userCollections);
        }

        [Authorize]
        public IActionResult CreateCollectionView()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> EditCollection(int id)
        {
            var collection  = await _dbContext.Collections.FindAsync(id);
            if (collection == null)
                return NotFound();
            return View(collection);
        }

        private async Task<string> UploadImage(IFormFile image)
        {
            var imageExtension = Path.GetExtension(image.FileName);
            string imageName = $"colleman-{DateTime.Now.ToString("yyyyMMddHHmmss")}{imageExtension}"; 
            return await _cloud.UploadImageAsync(image, imageName);
        }

        public async Task<IActionResult> CreateCollection(CreateCollectionViewModel ccvm)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(ccvm.Name) || String.IsNullOrEmpty(ccvm.Description))
                {
                    ModelState.AddModelError(string.Empty, "Full name and password cannot be empty");
                }
                var existingCollection = _dbContext.Collections.FirstOrDefault(x => x.Name == ccvm.Name);
                if (existingCollection != null)
                {
                    ModelState.AddModelError("Email", "This collection already exists!");
                    return View(ccvm);
                }
                else if (ccvm.ImageFile is not null)
                {
                    var collection = new Collection
                    {
                        Name = ccvm.Name,
                        Description = ccvm.Description,
                        ImageUrl = await UploadImage(ccvm.ImageFile),
                        Category = (Category)Enum.Parse(typeof(Category), ccvm.Category, true),
                        User = await _userManager.GetUserAsync(User)
                    };
                    await _dbContext.Collections.AddAsync(collection);
                    _dbContext.SaveChanges();
                    await _userManager.UpdateAsync(await _userManager.GetUserAsync(User));
                    return RedirectToAction("MyCollections", "User");
                }
                else
                {
                    var collection = new Collection
                    {
                        Name = ccvm.Name,
                        Description = ccvm.Description,
                        Category = (Category)Enum.Parse(typeof(Category), ccvm.Category, true),
                        User = await _userManager.GetUserAsync(User)
                    };
                    await _dbContext.Collections.AddAsync(collection);
                    _dbContext.SaveChanges();
                    await _userManager.UpdateAsync(await _userManager.GetUserAsync(User));
                    return RedirectToAction("MyCollections", "User");
                }
            }
            return View(ccvm);
        }
    }
}
