using Microsoft.EntityFrameworkCore;

namespace Models{
    public class EvidencijaContext:DbContext{
        public EvidencijaContext(DbContextOptions options):base(options){}
        public DbSet<Radnik> Radnici{get;set;}
    }
}