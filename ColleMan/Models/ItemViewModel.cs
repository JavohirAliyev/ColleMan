namespace ColleMan.Models
{
    public class ItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public DateTime DateCreated { get; set; }
        public int LikeCount { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }

}

