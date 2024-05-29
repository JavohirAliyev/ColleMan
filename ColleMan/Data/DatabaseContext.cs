using ColleMan.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace ColleMan.Data
{
    public class DatabaseContext : IdentityDbContext<IdentityUser>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        DbSet<Collection> Collections { get; set; }
        DbSet<Comment> Comments { get; set; }
        DbSet<Item> Items { get; set; }
        DbSet<Like> Likes { get; set; }
        DbSet<Tag> Tags { get; set; }
    }
}
