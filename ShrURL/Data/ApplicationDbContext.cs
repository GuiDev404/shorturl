using Microsoft.EntityFrameworkCore;
using ShrURL.Models;

namespace ShrURL.Data {
    public class ApplicationDbContext : DbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {}

        public DbSet<ShortURL> ShortURLs { get; set; }
    }
}
