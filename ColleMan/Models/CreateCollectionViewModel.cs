﻿namespace ColleMan.Models
{
    public class CreateCollectionViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public IFormFile? ImageFile { get; set; }

    }
}
