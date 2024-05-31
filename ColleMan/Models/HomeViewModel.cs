namespace ColleMan.Models
{
    public class HomeViewModel
    {
        public List<Item> RecentItems { get; set; }
        public List<Collection> LargestCollections { get; set; }
        public List<string> AllTags { get; set; }
    }

}
