using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using English.Entity.Entities;

namespace English.Repository.Data;

/// <summary>
/// DbContext chính của ứng dụng EnglishLocker cấu hình theo cấu trúc Dictionary Cluster.
/// </summary>
public class AppDbContext : DbContext
{
    // ── Private Value Converter & Comparer (bắt đầu bằng tiền tố _) ──
    private static readonly ValueConverter<List<string>, string> _stringListConverter = new(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

    private static readonly ValueComparer<List<string>> _stringListComparer = new(
        (c1, c2) => c1 != null && c2 != null ? c1.SequenceEqual(c2) : c1 == c2,
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
        c => c.ToList());

    // ── DbSets ──
    public DbSet<Vocabulary> Vocabularies => Set<Vocabulary>();
    public DbSet<VocabularyMeaning> VocabularyMeanings => Set<VocabularyMeaning>();
    public DbSet<MeaningExample> MeaningExamples => Set<MeaningExample>();

    public static string CurrentDatabasePath { get; set; } = "englishlocker.db";

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public void SwitchDatabase(string newDatabasePath)
    {
        CurrentDatabasePath = newDatabasePath;
        Database.CloseConnection();
        Database.SetConnectionString($"Data Source={newDatabasePath}");
        ChangeTracker.Clear();
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={CurrentDatabasePath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ───────────────────────────────────────────────
        // 1. Vocabulary Entity Configuration
        // ───────────────────────────────────────────────
        modelBuilder.Entity<Vocabulary>(entity =>
        {
            entity.ToTable("Vocabularies");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.WordText)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(e => e.Phonetic_UK)
                  .HasMaxLength(100);

            entity.Property(e => e.Phonetic_US)
                  .HasMaxLength(100);

            entity.Property(e => e.AudioPath_UK)
                  .HasMaxLength(500);

            entity.Property(e => e.AudioPath_US)
                  .HasMaxLength(500);

            entity.Property(e => e.Level)
                  .HasConversion<int>();

            // Value Converter map List<string> sang JSON text cho SQLite
            entity.Property(e => e.WordFamily)
                  .HasConversion(_stringListConverter, _stringListComparer);

            // Quan hệ 1-N: Vocabulary (1) -> (N) VocabularyMeaning
            entity.HasMany(e => e.Meanings)
                  .WithOne(m => m.Vocabulary)
                  .HasForeignKey(m => m.VocabularyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ───────────────────────────────────────────────
        // 2. VocabularyMeaning Entity Configuration
        // ───────────────────────────────────────────────
        modelBuilder.Entity<VocabularyMeaning>(entity =>
        {
            entity.ToTable("VocabularyMeanings");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.WordClass)
                  .HasConversion<int>();

            entity.Property(e => e.Definition_EN)
                  .IsRequired()
                  .HasMaxLength(1000);

            entity.Property(e => e.Definition_VI)
                  .IsRequired()
                  .HasMaxLength(1000);

            entity.Property(e => e.Context)
                  .HasConversion<int>();

            // Value Converter map List<string> sang JSON text cho SQLite
            entity.Property(e => e.Synonyms)
                  .HasConversion(_stringListConverter, _stringListComparer);

            entity.Property(e => e.Antonyms)
                  .HasConversion(_stringListConverter, _stringListComparer);

            // Quan hệ 1-N: VocabularyMeaning (1) -> (N) MeaningExample
            entity.HasMany(e => e.Examples)
                  .WithOne(ex => ex.Meaning)
                  .HasForeignKey(ex => ex.MeaningId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ───────────────────────────────────────────────
        // 3. MeaningExample Entity Configuration
        // ───────────────────────────────────────────────
        modelBuilder.Entity<MeaningExample>(entity =>
        {
            entity.ToTable("MeaningExamples");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Sentence_EN)
                  .IsRequired()
                  .HasMaxLength(1000);

            entity.Property(e => e.Sentence_VI)
                  .IsRequired()
                  .HasMaxLength(1000);

            entity.Property(e => e.HighlightedTarget)
                  .HasMaxLength(200);
        });
    }
}
