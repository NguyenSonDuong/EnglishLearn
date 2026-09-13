using Microsoft.EntityFrameworkCore;
using English.Entity.Entities;

namespace English.Repository.Data;

/// <summary>DbContext chính của ứng dụng EnglishLocker.</summary>
public class AppDbContext : DbContext
{
    public DbSet<AppConfiguration> AppConfigurations => Set<AppConfiguration>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<LearningMaterial> LearningMaterials => Set<LearningMaterial>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<StudyRecord> StudyRecords => Set<StudyRecord>();
    public DbSet<StudyHistory> StudyHistories => Set<StudyHistory>();
    public DbSet<EmergencyLog> EmergencyLogs => Set<EmergencyLog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=englishlocker.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── AppConfiguration: PK = ConfigKey (string) ──
        modelBuilder.Entity<AppConfiguration>(entity =>
        {
            entity.ToTable("AppConfigurations");
            entity.HasKey(e => e.ConfigKey);
        });

        // ── Deck ──
        modelBuilder.Entity<Deck>(entity =>
        {
            entity.ToTable("Decks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // ── LearningMaterial ──
        modelBuilder.Entity<LearningMaterial>(entity =>
        {
            entity.ToTable("LearningMaterials");
            entity.HasKey(e => e.Id);

            // 1-N: Deck → LearningMaterials
            entity.HasOne(e => e.Deck)
                  .WithMany(d => d.LearningMaterials)
                  .HasForeignKey(e => e.DeckId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Enum lưu dạng int
            entity.Property(e => e.CategoryType)
                  .HasConversion<int>();
        });

        // ── Question ──
        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Questions");
            entity.HasKey(e => e.Id);

            // 1-N: LearningMaterial → Questions
            entity.HasOne(e => e.LearningMaterial)
                  .WithMany(lm => lm.Questions)
                  .HasForeignKey(e => e.LearningMaterialId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.TestType)
                  .HasConversion<int>();
        });

        // ── StudyRecord (1-1 với Question) ──
        modelBuilder.Entity<StudyRecord>(entity =>
        {
            entity.ToTable("StudyRecords");
            entity.HasKey(e => e.Id);

            // 1-1: Question ↔ StudyRecord
            entity.HasOne(e => e.Question)
                  .WithOne(q => q.StudyRecord)
                  .HasForeignKey<StudyRecord>(e => e.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint trên QuestionId (đảm bảo 1-1)
            entity.HasIndex(e => e.QuestionId)
                  .IsUnique();

            entity.Property(e => e.EasinessFactor)
                  .HasDefaultValue(2.5);
        });

        // ── StudyHistory (1-N với Question) ──
        modelBuilder.Entity<StudyHistory>(entity =>
        {
            entity.ToTable("StudyHistories");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Question)
                  .WithMany(q => q.StudyHistories)
                  .HasForeignKey(e => e.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── EmergencyLog ──
        modelBuilder.Entity<EmergencyLog>(entity =>
        {
            entity.ToTable("EmergencyLogs");
            entity.HasKey(e => e.Id);
        });
    }
}
