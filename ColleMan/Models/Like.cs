using Microsoft.AspNetCore.Identity;

namespace ColleMan.Models
{
    public class Like
    {
        public int Id { get; set; }
        public ApplicationUser User { get; set; }
        public Item Item { get; set; }

    }
}
