using ColleMan.Data;
using ColleMan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ColleMan.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminPanel()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Block(string userIds)
        {
            List<Guid>? selectedUsersList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Guid>>(userIds);

            foreach (Guid userGuid in selectedUsersList)
            {
                var user = await _userManager.FindByIdAsync(userGuid.ToString());
                if (user != null)
                {
                    user.IsBlocked = Status.Blocked;
                    await _userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction("AdminPanel", "Admin");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unblock(string userIds)
        {
            List<Guid>? selectedUsersList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Guid>>(userIds);

            foreach (Guid userGuid in selectedUsersList)
            {
                var user = await _userManager.FindByIdAsync(userGuid.ToString());
                if (user != null)
                {
                    user.IsBlocked = Status.Active;
                    await _userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction("AdminPanel", "Admin");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string userIds)
        {
            List<Guid>? selectedUsersList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Guid>>(userIds);

            foreach (Guid userGuid in selectedUsersList)
            {
                var user = await _userManager.FindByIdAsync(userGuid.ToString());
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                    await _userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction("AdminPanel", "Admin");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Promote(string userIds)
        {
            List<Guid>? selectedUsersList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Guid>>(userIds);

            foreach (Guid userGuid in selectedUsersList)
            {
                var user = await _userManager.FindByIdAsync(userGuid.ToString());
                if (user != null)
                {
                    await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
                    await _userManager.AddToRoleAsync(user, "Admin");
                    await _userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction("AdminPanel", "Admin");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Demote(string userIds)
        {
            List<Guid>? selectedUsersList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Guid>>(userIds);

            foreach (Guid userGuid in selectedUsersList)
            {
                var user = await _userManager.FindByIdAsync(userGuid.ToString());
                if (user != null)
                {
                    await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
                    await _userManager.AddToRoleAsync(user, "User");
                    await _userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction("AdminPanel", "Admin");
        }

    }
}
