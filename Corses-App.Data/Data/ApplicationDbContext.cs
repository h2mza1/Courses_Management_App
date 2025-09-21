using Courses_App.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Configuration;
using System.Reflection.Emit;

namespace Corses_App.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
      
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<Instructor>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.NoAction) ;
            builder.Entity<Categeory>()
                .HasMany(c => c.Courses)
                .WithOne(c => c.Categeory)
                .HasForeignKey(c => c.CategeoryId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Course>()
                .Property(c => c.IsActive)
                .HasDefaultValue(true);

            builder.Entity<Instructor>()
                .Property(c=> c.Major)
                .HasDefaultValue("FullStack ");
            builder.Entity<Course>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");


            builder.Entity<Enrollment>()
                .HasIndex(e => e.CourseId);
            builder.Entity<Course>()
                .HasIndex(c => c.CategeoryId);
            builder.Entity<Course>()
                .HasIndex(c => c.InstructorId);
            builder.Entity<User>()
                 .HasIndex(u => u.IsDeleted);
            builder.Entity<Course>()
                .HasIndex(c =>  c.IsDeleted );



        }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<CourseVideos> videos { get; set; }
        public DbSet<Categeory> Categeories { get; set; }
    }
}
