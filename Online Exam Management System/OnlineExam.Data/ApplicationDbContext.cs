using Microsoft.EntityFrameworkCore;

namespace OnlineExam.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<UserAnswer> UserAnswers => Set<UserAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Role).HasConversion<byte>();
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.Property(x => x.Text).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Topic).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Type).HasConversion<byte>();
            entity.HasMany(x => x.Options).WithOne(x => x.Question).HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.Property(x => x.Text).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Attempt>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<byte>();
            entity.HasMany(x => x.Answers).WithOne(x => x.Attempt).HasForeignKey(x => x.AttemptId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserAnswer>()
            .HasIndex(x => new { x.AttemptId, x.QuestionId })
            .IsUnique();

        modelBuilder.Entity<UserAnswer>()
            .HasOne(x => x.Question)
            .WithMany()
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
