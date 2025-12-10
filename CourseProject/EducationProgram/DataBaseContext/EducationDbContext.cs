using EducationProgram.Models;
using Microsoft.EntityFrameworkCore;

namespace EducationProgram.DataBaseContext;

public partial class EducationDbContext : DbContext
{
    public EducationDbContext()
    {
    }

    public EducationDbContext(DbContextOptions<EducationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Leaderboard> Leaderboards { get; set; }

    public virtual DbSet<Paragraph> Paragraphs { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<TestResult> TestResults { get; set; }

    public virtual DbSet<TestResultDisplay> TestResultDisplays { get; set; }

    public virtual DbSet<Theme> Themes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseMySql("server=mysql.softsols.ru;port=63307;user id=admin;password=pOM8K8HA99685dXI;database=mydb", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.44-mysql"));
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb3_general_ci")
            .HasCharSet("utf8mb3");

        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PRIMARY");

            entity.ToTable("answer");

            entity.HasIndex(e => e.QuestionId, "fk_answer_question2_idx");

            entity.Property(e => e.AnswerId).HasColumnName("answer_id");
            entity.Property(e => e.AnswerText)
                .HasColumnType("text")
                .HasColumnName("answer_text");
            entity.Property(e => e.IsCorrect).HasColumnName("is_correct");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("fk_answer_question2");
        });

        modelBuilder.Entity<Leaderboard>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("leaderboard");

            entity.Property(e => e.GroupName)
                .HasMaxLength(45)
                .HasColumnName("group_name");
            entity.Property(e => e.Login)
                .HasMaxLength(25)
                .HasColumnName("login");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(45)
                .HasColumnName("patronymic");
            entity.Property(e => e.Rank).HasColumnName("rank");
            entity.Property(e => e.Surname)
                .HasMaxLength(45)
                .HasColumnName("surname");
            entity.Property(e => e.TotalScore)
                .HasPrecision(32)
                .HasColumnName("total_score");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Paragraph>(entity =>
        {
            entity.HasKey(e => e.ParagraphId).HasName("PRIMARY");

            entity.ToTable("paragraph");

            entity.HasIndex(e => e.ThemeId, "fk_paragraph_theme1_idx");

            entity.Property(e => e.ParagraphId).HasColumnName("paragraph_id");
            entity.Property(e => e.ParagraphTitle)
                .HasMaxLength(80)
                .HasColumnName("paragraph_title");
            entity.Property(e => e.Path)
                .HasMaxLength(300)
                .HasColumnName("path");
            entity.Property(e => e.ThemeId).HasColumnName("theme_id");

            entity.HasOne(d => d.Theme).WithMany(p => p.Paragraphs)
                .HasForeignKey(d => d.ThemeId)
                .HasConstraintName("fk_paragraph_theme1");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PRIMARY");

            entity.ToTable("question");

            entity.HasIndex(e => e.TestId, "fk_question_test1_idx");

            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Points)
                .HasDefaultValueSql("'5'")
                .HasColumnName("points");
            entity.Property(e => e.QuestionText)
                .HasColumnType("text")
                .HasColumnName("question_text");
            entity.Property(e => e.TestId).HasColumnName("test_id");

            entity.HasOne(d => d.Test).WithMany(p => p.Questions)
                .HasForeignKey(d => d.TestId)
                .HasConstraintName("fk_question_test1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PRIMARY");

            entity.ToTable("role");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(45)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.SectionId).HasName("PRIMARY");

            entity.ToTable("section");

            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.SectionTitle)
                .HasMaxLength(45)
                .HasColumnName("section_title");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PRIMARY");

            entity.ToTable("test");

            entity.HasIndex(e => e.ThemeId, "fk_test_theme1_idx");

            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.MaxAttempts).HasColumnName("max_attempts");
            entity.Property(e => e.TestName)
                .HasMaxLength(100)
                .HasColumnName("test_name");
            entity.Property(e => e.ThemeId).HasColumnName("theme_id");

            entity.HasOne(d => d.Theme).WithMany(p => p.Tests)
                .HasForeignKey(d => d.ThemeId)
                .HasConstraintName("fk_test_theme1");
        });

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.TestId, e.AttemptNumber })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

            entity.ToTable("test_result");

            entity.HasIndex(e => e.TestId, "fk_user_has_test_test1_idx");

            entity.HasIndex(e => e.UserId, "fk_user_has_test_user1_idx");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.AttemptNumber).HasColumnName("attempt_number");
            entity.Property(e => e.PassedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("passed_at");
            entity.Property(e => e.Score).HasColumnName("score");

            entity.HasOne(d => d.Test).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.TestId)
                .HasConstraintName("fk_user_has_test_test1");

            entity.HasOne(d => d.User).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_user_has_test_user1");
        });

        modelBuilder.Entity<TestResultDisplay>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("test_result_display");

            entity.Property(e => e.AttemptNumber).HasColumnName("attempt_number");
            entity.Property(e => e.Grade).HasColumnName("grade");
            entity.Property(e => e.MaxScore)
                .HasPrecision(32)
                .HasColumnName("max_score");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.PassedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("passed_at");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(45)
                .HasColumnName("patronymic");
            entity.Property(e => e.Percent)
                .HasPrecision(16, 2)
                .HasColumnName("percent");
            entity.Property(e => e.Score).HasColumnName("score");
            entity.Property(e => e.Surname)
                .HasMaxLength(45)
                .HasColumnName("surname");
            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.TestName)
                .HasMaxLength(100)
                .HasColumnName("test_name");
            entity.Property(e => e.UserGroup)
                .HasMaxLength(45)
                .HasColumnName("user_group");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserLogin)
                .HasMaxLength(25)
                .HasColumnName("user_login");
        });

        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.ThemeId).HasName("PRIMARY");

            entity.ToTable("theme");

            entity.HasIndex(e => e.SectionId, "fk_theme_section1_idx");

            entity.Property(e => e.ThemeId).HasColumnName("theme_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.ThemeTitle)
                .HasMaxLength(45)
                .HasColumnName("theme_title");

            entity.HasOne(d => d.Section).WithMany(p => p.Themes)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("fk_theme_section1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("user");

            entity.HasIndex(e => e.GroupId, "fk_user_group1_idx");

            entity.HasIndex(e => e.RoleId, "fk_user_role2_idx");

            entity.HasIndex(e => e.Login, "login_UNIQUE").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.Login)
                .HasMaxLength(25)
                .HasColumnName("login");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(20)
                .HasColumnName("password");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(45)
                .HasColumnName("patronymic");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Surname)
                .HasMaxLength(45)
                .HasColumnName("surname");

            entity.HasOne(d => d.Group).WithMany(p => p.Users)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_user_group1");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("fk_user_role2");
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PRIMARY");

            entity.ToTable("user_group");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.GroupName)
                .HasMaxLength(45)
                .HasColumnName("group_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
