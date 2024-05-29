using Microsoft.AspNetCore.Identity;

namespace ColleMan.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Collection>? Collections { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<Like>? Likes { get; set; }
        public bool IsBlocked { get; set; } = false;
    }
}