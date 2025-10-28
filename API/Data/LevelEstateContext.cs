#nullable disable
using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace API.Data
{
    public class LevelEstateContext : DbContext
    {

        public LevelEstateContext(DbContextOptions<LevelEstateContext> options) : base(options) { }

        public DbSet<Agent> Agents { get; set; } = null;

        public DbSet<AgentRanking> AgentRankings { get; set; } = null;
        public DbSet<Badge> Badges { get; set; } = null;

        public DbSet<Chat> Chats { get; set; } = null;

        public DbSet<Client> Clients { get; set; } = null;

        public DbSet<Message> Messages { get; set; } = null;

        public DbSet<Notification> Notifications { get; set; } = null;

        public DbSet<Picture> Pictures { get; set; } = null;

        public DbSet<Property> Properties { get; set; } = null;

        public DbSet<Ranking> Rankings { get; set; } = null;

        public DbSet<Feedback> Feedback { get; set; } = null;

        public DbSet<User> Users { get; set; } = null;

        public DbSet<Visit> Visits { get; set; } = null;



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //Message
            modelBuilder.Entity<Message>()
                .HasOne(n => n.Sender)
                .WithMany()
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            //Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Property)
                .WithMany()
                .HasForeignKey(c => c.PropertyId)
                .OnDelete(DeleteBehavior.NoAction);

            //Chat
            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Agent)
                .WithMany()
                .HasForeignKey(c => c.AgentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Client)
                .WithMany()
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Chat>()
                .HasOne(a => a.Agent)
                .WithMany(c => c.Chats)
                .HasForeignKey(c => c.AgentId);

            modelBuilder.Entity<Chat>()
                .HasOne(a => a.Client)
                .WithMany(c => c.Chats)
                .HasForeignKey(c => c.ClientId);

            //Visit
            modelBuilder.Entity<Visit>()
               .HasOne(n => n.Agent)
               .WithMany()
               .HasForeignKey(c => c.AgentId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Visit>()
                .HasOne(n => n.Client)
                .WithMany()
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Visit>()
                .HasOne(a => a.Agent)
                .WithMany(b => b.Visits)
                .HasForeignKey(a => a.AgentId);

            modelBuilder.Entity<Visit>()
                .HasOne(a => a.Client)
                .WithMany(b => b.Visits)
                .HasForeignKey(a => a.ClientId);

            //Generate date on add and ignore on update
            modelBuilder.Entity<Property>()
                .Property(p => p.CreatedAt)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            //Generate date on add and ignore on update
            modelBuilder.Entity<Visit>()
                .Property(v => v.CreatedAt)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            //Generate date on add and ignore on update
            modelBuilder.Entity<Feedback>()
                .Property(c => c.CreatedAt)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            //Generate date on add and ignore on update
            modelBuilder.Entity<Message>()
                .Property(c => c.SendDate)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            //Generate date on add and ignore on update
            modelBuilder.Entity<Chat>()
                .Property(c => c.CreatedAt)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            //Generate date on add and ignore on update
            modelBuilder.Entity<User>()
                .Property(c => c.RegistrationDate)
                .ValueGeneratedOnAdd()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            //Favorite
            modelBuilder.Entity<Client>()
                .HasMany(c => c.Favorite)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "Favorites",
                    j => j
                        .HasOne<Property>()
                        .WithMany()
                        .HasForeignKey("PropertyId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<Client>()
                        .WithMany()
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.NoAction)
                );

            base.OnModelCreating(modelBuilder);
        }
    }
}
