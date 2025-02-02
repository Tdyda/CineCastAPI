using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VideoStreamingAPI.Models;
using VideoStreamingAPI.Models.VideoStreamingAPI.Models;

namespace VideoStreamingAPI.Data
{
    public class VideoStreamingDbContext : IdentityDbContext<AppUserModel>
    {
        public VideoStreamingDbContext(DbContextOptions<VideoStreamingDbContext> options) : base(options) { }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<MovieTag> MovieTags { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Tag> Tags { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MovieTag>()
                .HasKey(mt => new { mt.MovieId, mt.TagId });

            modelBuilder.Entity<MovieTag>()
                .HasOne(mt => mt.Movie)
                .WithMany(m => m.MovieTags)
                .HasForeignKey(mt => mt.MovieId);

            modelBuilder.Entity<MovieTag>()
                .HasOne(mt => mt.Tag)
                .WithMany(t => t.MovieTags)
                .HasForeignKey(mt => mt.TagId);

            modelBuilder.Entity<MovieActor>()
                .HasKey(ma => new {ma.MovieId, ma.ActorId});

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Actor)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId);

            modelBuilder.Entity<MovieActor>()
                .HasOne(ma => ma.Movie)
                .WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.MovieId);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("your_connection_string")
                              .LogTo(Console.WriteLine, LogLevel.Information); // logowanie zapytań do konsoli
            }
        }
    }
}
