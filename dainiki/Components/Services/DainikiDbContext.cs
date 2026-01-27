namespace dainiki.Components.Services
{
    using dainiki.Components.Models;
    using Microsoft.EntityFrameworkCore;

    public class DainikiDbContext : DbContext
    {
        public DainikiDbContext(DbContextOptions<DainikiDbContext> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Journal> Journals { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Mood> Moods { get; set; }
        public DbSet<JournalTag> JournalTags { get; set; }
        public DbSet<JournalMood> JournalMoods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>()
                .HasKey(user => user.username);

            modelBuilder.Entity<Users>()
                .Property(user => user.username)
                .HasMaxLength(64)
                .IsRequired();

            modelBuilder.Entity<Users>()
                .Property(user => user.password_hash)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .Property(category => category.category_name)
                .HasMaxLength(64)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .Property(category => category.category_type)
                .HasMaxLength(16)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .HasKey(category => category.category_id);

            modelBuilder.Entity<Tag>()
                .Property(tag => tag.tag_name)
                .HasMaxLength(64)
                .IsRequired();

            modelBuilder.Entity<Tag>()
                .HasKey(tag => tag.tag_id);

            modelBuilder.Entity<Mood>()
                .Property(mood => mood.mood_name)
                .HasMaxLength(64)
                .IsRequired();

            modelBuilder.Entity<Mood>()
                .HasKey(mood => mood.mood_id);

            modelBuilder.Entity<Journal>()
                .HasKey(journal => journal.journal_id);

            modelBuilder.Entity<Journal>()
                .Property(journal => journal.journal_title)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Journal>()
                .Property(journal => journal.journal_content_url)
                .IsRequired();

            modelBuilder.Entity<Journal>()
                .Property(journal => journal.created_at)
                .IsRequired();

            modelBuilder.Entity<Journal>()
                .Property(journal => journal.updated_at)
                .IsRequired();

            modelBuilder.Entity<Journal>()
                .Property(journal => journal.is_pinned)
                .HasDefaultValue(false);

            modelBuilder.Entity<Journal>()
                .HasIndex(journal => new { journal.user_id, journal.journal_date })
                .IsUnique();

            modelBuilder.Entity<JournalTag>()
                .HasKey(journalTag => new { journalTag.journal_id, journalTag.tag_id });

            modelBuilder.Entity<JournalMood>()
                .HasKey(journalMood => new { journalMood.journal_id, journalMood.mood_id });

            modelBuilder.Entity<JournalMood>()
                .Property(journalMood => journalMood.mood_role)
                .HasMaxLength(16)
                .IsRequired();

            modelBuilder.Entity<Journal>()
                .HasOne(journal => journal.user)
                .WithMany(user => user.Journals)
                .HasForeignKey(journal => journal.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Journal>()
                .HasOne(journal => journal.category)
                .WithMany(category => category.Journals)
                .HasForeignKey(journal => journal.category_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Mood>()
                .HasOne(mood => mood.category)
                .WithMany(category => category.Moods)
                .HasForeignKey(mood => mood.category_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JournalTag>()
                .HasOne(journalTag => journalTag.journal)
                .WithMany(journal => journal.JournalTags)
                .HasForeignKey(journalTag => journalTag.journal_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JournalTag>()
                .HasOne(journalTag => journalTag.tag)
                .WithMany(tag => tag.JournalTags)
                .HasForeignKey(journalTag => journalTag.tag_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JournalMood>()
                .HasOne(journalMood => journalMood.journal)
                .WithMany(journal => journal.JournalMoods)
                .HasForeignKey(journalMood => journalMood.journal_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JournalMood>()
                .HasOne(journalMood => journalMood.mood)
                .WithMany(mood => mood.JournalMoods)
                .HasForeignKey(journalMood => journalMood.mood_id)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
