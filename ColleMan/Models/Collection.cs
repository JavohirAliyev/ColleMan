using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColleMan.Models
{
    public class Collection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Category Category { get; set; }
        [DataType(DataType.ImageUrl)]
        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public ApplicationUser User { get; set; }
        public List<Item> Items { get; set; }
        //public bool custom_string1_state { get; set; }
        //public string? custom_string1_name { get; set; }
        //public bool custom_string2_state { get; set; }
        //public string? custom_string2_name { get; set; }
        //public bool custom_string3_state { get; set; }
        //public string? custom_string3_name { get; set; }
        //public bool custom_int1_state { get; set; }
        //public int? custom_int1_name { get; set; }
        //public bool custom_int2_state { get; set; }
        //public int? custom_int2_name { get; set; }
        //public bool custom_int3_state { get; set; }
        //public int? custom_int3_name { get; set; }
        //public bool custom_bool1_state { get; set; }
        //public bool? custom_bool1_name { get; set; }
        //public bool custom_bool2_state { get; set; }
        //public bool? custom_bool2_name { get; set; }
        //public bool custom_bool3_state { get; set; }
        //public bool? custom_bool3_name { get; set; }
        //public bool custom_datetime1_state { get; set; }
        //public DateTime? custom_datetime1_name { get; set; }
        //public bool custom_datetime2_state { get; set; }
        //public DateTime? custom_datetime2_name { get; set; }
        //public bool custom_datetime3_state { get; set; }
        //public DateTime? custom_datetime3_name { get; set; }
    }
    public enum Category
    {
        Books,
        Movies,
        Songs,
        Signs,
        Coins,
        Other
    }
}