using ColleMan.Data;
using ColleMan.Interfaces;
using ColleMan.Migrations;
using ColleMan.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
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

        [Route("User/GetCollections/{userId}")]
        public async Task<IActionResult> GetCollections(string userId)
        {
            var collectionOwner = await _userManager.FindByIdAsync(userId);
            if (collectionOwner == null)
                return NotFound();
            var userCollections = _dbContext.Collections
                .Include(c => c.User)
                .Where(c => c.User.Id == collectionOwner.Id)
                .ToList();
            return View(userCollections);
        }
        [Authorize]
        public IActionResult CreateCollectionView()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditCollection(int id)
        {
            var collection  = await _dbContext.Collections.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
            if (collection == null)
                return NotFound();
            return View(collection);
        }
        [HttpPost]
        public async Task<IActionResult> EditCollection(Collection ecvm)
        {
            Collection? collection = await _dbContext.Collections
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == ecvm.Id);
            if (!User.IsInRole("Admin") && (await _userManager.GetUserAsync(User)) != collection.User)
                return RedirectToAction("GetCollections", "User", new {collection.User.Id});
            else if (collection == null)
            {
                ModelState.AddModelError(String.Empty, "Collection not found!");
                return View(collection);
            }
            else if ((await _dbContext.Collections.FirstOrDefaultAsync(x => x.Name == ecvm.Name)) is not null)
            {
                ModelState.AddModelError(String.Empty, "Such collection already exists");
                return View(collection);
            }
            else if (String.IsNullOrEmpty(ecvm.Name) || String.IsNullOrEmpty(ecvm.Description))
            {
                ModelState.AddModelError(string.Empty, "Name and Description cannot be empty");
                return View(collection);
            }
            else if (ecvm.ImageFile is not null && collection.ImageFile is null)
            {
                collection.Name = ecvm.Name;
                collection.Description = ecvm.Description;
                collection.Category = ecvm.Category;
                collection.ImageUrl = await UploadImage(ecvm.ImageFile);

                await _dbContext.SaveChangesAsync();
            }
            else if (ecvm.ImageFile is not null && collection.ImageFile is not null)
            {
                collection.Name = ecvm.Name;
                collection.Description = ecvm.Description;
                collection.Category = ecvm.Category;
                await _cloud.DeleteImageAsync(collection.ImageUrl);
                collection.ImageUrl = await UploadImage(ecvm.ImageFile);

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                collection.Name = ecvm.Name;
                collection.Description = ecvm.Description;
                collection.Category = ecvm.Category;

                await _dbContext.SaveChangesAsync();
            }
            
            return RedirectToAction("GetCollections", "User", new { collection.User.Id});
        }
        [Authorize]
        private async Task<string> UploadImage(IFormFile image)
        {
            var imageExtension = Path.GetExtension(image.FileName);
            string imageName = $"colleman-{DateTime.Now.ToString("yyyyMMddHHmmss")}{imageExtension}"; 
            return await _cloud.UploadImageAsync(image, imageName);
        }
        [Authorize]
        public async Task<IActionResult> CreateCollection(CreateCollectionViewModel ccvm)
        {
            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(ccvm.Name) || String.IsNullOrEmpty(ccvm.Description))
                {
                    ModelState.AddModelError(string.Empty, "Name and Description cannot be empty");
                    return View(ccvm);
                }
                var currentUser = await _userManager.GetUserAsync(User);
                var userCollections = _dbContext.Collections
                    .Include(c => c.User)
                    .Where(c => c.User.Id == currentUser.Id)
                    .ToList();
                var existingCollection = userCollections.FirstOrDefault(x => x.Name == ccvm.Name);
                if (existingCollection != null)
                {
                    ModelState.AddModelError("Error", "This collection already exists!");
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
                    await _dbContext.SaveChangesAsync();
                    await _userManager.UpdateAsync(await _userManager.GetUserAsync(User));
                    return RedirectToAction("GetCollections", "User", new { collection.User.Id });
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
                    await _dbContext.SaveChangesAsync();
                    await _userManager.UpdateAsync(await _userManager.GetUserAsync(User));
                    return RedirectToAction("GetCollections", "User", new { collection.User.Id });
                }
            }
            return View(ccvm);
        }


        [Route("User/GetItems/{collId}")]
        public IActionResult GetItems(int collId)
        {
            var coll = _dbContext.Collections
                .Include(_ => _.User)
                .FirstOrDefault(c => c.Id == collId);
            List<Item> items = _dbContext.Items.Where(x => x.Collection.Id == collId).ToList();
            return View(items);
        }

        [Authorize]
        public async Task<IActionResult> DeleteCollection(int id)
        {
            var selectedCollection = await _dbContext.Collections
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);

            if (!User.IsInRole("Admin") && (await _userManager.GetUserAsync(User)) != selectedCollection.User)
                return RedirectToPage("~/AccessDenied.aspx");
            if (selectedCollection != null && selectedCollection.ImageFile != null)
            {
                await _cloud.DeleteImageAsync(selectedCollection.ImageUrl);
                _dbContext.Collections.Remove(selectedCollection);
                await _dbContext.SaveChangesAsync();
            }
            else if (selectedCollection != null)
            {
                _dbContext.Collections.Remove(selectedCollection);
                await _dbContext.SaveChangesAsync();
            }
            ModelState.AddModelError("Id", "There was a problem with deleting the record!");
            return RedirectToAction("GetCollections", "User", new { selectedCollection.User.Id });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CreateItem(string collectionId)
        {
            Collection col = await _dbContext.Collections.FindAsync(collectionId);
            return View(col);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateItem(string collectionId, CreateItemViewModel civm)
        {
            Collection? col = await _dbContext.Collections.FindAsync(collectionId);
            ApplicationUser collectionOwner = col.User;
            ApplicationUser currentUser = await _userManager.GetUserAsync(User);
            if (col == null)
            {
                ModelState.AddModelError("Choose Collection", "Collection is not chosen");
                return View(civm);
            }
            else if ((User.IsInRole("Admin") || collectionOwner.Id == currentUser.Id))
            {
                var existingItem = collectionOwner.Collections.FirstOrDefault(x => x.Name == civm.Name);
                if (existingItem != null)
                {
                    ModelState.AddModelError("Error", "This item already exists!");
                    return View(civm);
                }
                else
                {
                    var item = new Item
                    {
                        Name = civm.Name,
                        Tags = civm.Tags,
                        Collection = col,
                        DateCreated = DateTime.Now,
                    };
                    await _dbContext.Items.AddAsync(item);
                    _dbContext.SaveChanges();
                    await _userManager.UpdateAsync(await _userManager.GetUserAsync(User));
                    return RedirectToAction($"GetItems/{col.Id}", "User");
                }
            }
            else
            {
                ModelState.AddModelError("Access Denied", "You don't have the authority to add in this collection");
                return View(civm);
            }
        }
    }
}
