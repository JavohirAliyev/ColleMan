namespace ColleMan.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public DateTime DateCreated { get; set; }
        public Collection Collection { get; set; }
        public ICollection<Like>? Likes { get; set; }
        public ICollection<Comment>? Comments { get; set; }
    }
}
