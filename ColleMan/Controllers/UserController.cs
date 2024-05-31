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
using NuGet.Common;

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
        public static int collectionIdItemCreate;
        public IActionResult Index()
        {
            return View();
        }

        [Route("User/GetCollections/{userId}")]
        public async Task<IActionResult> GetCollections(string userId)
        {
            var collectionOwner = await _userManager.Users
                .Include(c => c.Collections)
                .FirstOrDefaultAsync(c => c.Id == userId);
            if (collectionOwner == null)
                return NotFound();
            return View(collectionOwner);
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
            collectionIdItemCreate = collId;
            var coll = _dbContext.Collections
                .Include(c => c.User)
                .Include(c => c.Items)
                .ThenInclude(x => x.Tags)
                .FirstOrDefault(c => c.Id == collId);
            return View(coll);
        }

        [Route("User/GetItem/{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await _dbContext.Items
                .Include(i => i.Tags)
                .Include(i => i.Comments)
                .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            var viewModel = new ItemViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Tags = item.Tags.ToList(),
                DateCreated = item.DateCreated,
                LikeCount = item.LikeCount,
                Comments = item.Comments?.ToList() ?? new List<Comment>()
            };

            return View(viewModel);
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
        public async Task<IActionResult> CreateItem()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateItem(CreateItemViewModel civm)
        {
            Collection? col = await _dbContext.Collections
                .Include(c => c.User)
                .FirstOrDefaultAsync(x => x.Id == collectionIdItemCreate);
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
                    if (String.IsNullOrEmpty(civm.Name) || String.IsNullOrEmpty(civm.Tags))
                    {
                        ModelState.AddModelError(String.Empty, "Name OR Tags cannot be empty");
                        return View(civm);
                    }
                    else
                    {
                        var tagList = civm.Tags.Split(',')
                          .Select(tag => new Tag { Name = tag.Trim() })
                          .ToList();

                        var item = new Item
                        {
                            Name = civm.Name,
                            Tags = tagList,
                            Collection = col,
                            DateCreated = DateTime.Now,
                        };
                        await _dbContext.Items.AddAsync(item);
                        _dbContext.SaveChanges();
                        await _userManager.UpdateAsync(await _userManager.GetUserAsync(User));
                        return RedirectToAction("GetItems", "User", new {col.Id});
                    }
                }
            }
            else
            {
                ModelState.AddModelError("Access Denied", "You don't have the authority to add in this collection");
                return View(civm);
            }
        }

        [Route("User/DeleteItem/{itemId}")]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            var selectedItem = await _dbContext.Items
           .Include(c => c.Collection)
           .FirstOrDefaultAsync(c => c.Id == itemId);
            if (selectedItem != null)
            {
                var coll = selectedItem.Collection;
                if (!User.IsInRole("Admin") && (await _userManager.GetUserAsync(User)) != selectedItem.Collection.User)
                    return RedirectToPage("~/AccessDenied.aspx");
                else if (selectedItem != null)
                {
                    _dbContext.Items.Remove(selectedItem);
                    await _dbContext.SaveChangesAsync();
                    return RedirectToAction("GetItems", "User", new { coll.Id });
                }
                ModelState.AddModelError("Id", "There was a problem with deleting the record!");
                return RedirectToAction("GetItems", "User", new { coll.Id });
            }
            ModelState.AddModelError("Id", "There is no item under this ID!");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [Route("User/AddComment/{itemId}")]
        public async Task<IActionResult> AddCommentAsync(int itemId, Comment comment)
        {
            var selectedItem = await _dbContext.Items.FirstOrDefaultAsync(c => c.Id == itemId);
            comment.DateCreated = DateTime.Now;
            comment.Item = selectedItem;
            comment.User = await _userManager.GetUserAsync(User);
            await _dbContext.Comments.AddAsync(comment);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("GetItem", "User", new { selectedItem.Id });
        }
       
    }
}
