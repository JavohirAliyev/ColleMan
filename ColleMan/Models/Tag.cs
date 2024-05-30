namespace ColleMan.Models
{
    public class Tag
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<Item> Item { get; set; } = new List<Item>();

        public override string ToString()
        {
            return Name;
        }
    }
}
