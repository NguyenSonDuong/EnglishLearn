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

    public static string CurrentDatabasePath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "englishlocker.db");

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
        EnsureTablesCreated();
    }

    /// <summary>
    /// Đảm bảo tất cả các bảng của Dictionary Cluster (Vocabularies, VocabularyMeanings, MeaningExamples)
    /// luôn tồn tại, ngay cả khi kết nối tới các tệp cơ sở dữ liệu SQLite cũ được tạo từ các phiên bản trước.
    /// </summary>
    public void EnsureTablesCreated()
    {
        Database.EnsureCreated();

        const string sql = @"
CREATE TABLE IF NOT EXISTS ""Vocabularies"" (
    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_Vocabularies"" PRIMARY KEY,
    ""WordText"" TEXT NOT NULL,
    ""Description"" TEXT NULL,
    ""Phonetic_UK"" TEXT NULL,
    ""Phonetic_US"" TEXT NULL,
    ""AudioPath_UK"" TEXT NULL,
    ""AudioPath_US"" TEXT NULL,
    ""WordFamily"" TEXT NOT NULL,
    ""Level"" INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS ""VocabularyMeanings"" (
    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_VocabularyMeanings"" PRIMARY KEY,
    ""VocabularyId"" TEXT NOT NULL,
    ""WordClass"" INTEGER NOT NULL,
    ""Definition_EN"" TEXT NOT NULL,
    ""Definition_VI"" TEXT NOT NULL,
    ""Context"" INTEGER NOT NULL,
    ""Synonyms"" TEXT NOT NULL,
    ""Antonyms"" TEXT NOT NULL,
    CONSTRAINT ""FK_VocabularyMeanings_Vocabularies_VocabularyId"" FOREIGN KEY (""VocabularyId"") REFERENCES ""Vocabularies"" (""Id"") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ""MeaningExamples"" (
    ""Id"" TEXT NOT NULL CONSTRAINT ""PK_MeaningExamples"" PRIMARY KEY,
    ""MeaningId"" TEXT NOT NULL,
    ""Sentence_EN"" TEXT NOT NULL,
    ""Sentence_VI"" TEXT NOT NULL,
    ""HighlightedTarget"" TEXT NULL,
    CONSTRAINT ""FK_MeaningExamples_VocabularyMeanings_MeaningId"" FOREIGN KEY (""MeaningId"") REFERENCES ""VocabularyMeanings"" (""Id"") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ""IX_VocabularyMeanings_VocabularyId"" ON ""VocabularyMeanings"" (""VocabularyId"");
CREATE INDEX IF NOT EXISTS ""IX_MeaningExamples_MeaningId"" ON ""MeaningExamples"" (""MeaningId"");
";
        Database.ExecuteSqlRaw(sql);

        // Đảm bảo tương thích ngược: Bổ sung cột Description nếu kết nối vào DB SQLite đã tồn tại từ trước
        try
        {
            Database.ExecuteSqlRaw(@"ALTER TABLE ""Vocabularies"" ADD COLUMN ""Description"" TEXT NULL;");
        }
        catch
        {
            // Bỏ qua nếu cột Description đã tồn tại hoặc bảng vừa được khởi tạo mới
        }
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

            entity.Property(e => e.Description)
                  .HasMaxLength(2000);

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
