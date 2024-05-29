using Microsoft.AspNetCore.Identity;

namespace ColleMan.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime DateCreated { get; set; }
        public Item Item { get; set; }

    }
}
