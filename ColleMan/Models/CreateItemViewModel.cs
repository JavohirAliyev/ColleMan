namespace ColleMan.Models
{
    public class CreateItemViewModel
    {
        public string Name { get; set; }
        public ICollection<Tag> Tags { get; set; }
    }
}
