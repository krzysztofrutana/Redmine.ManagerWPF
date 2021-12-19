using Microsoft.EntityFrameworkCore;
using Redmine.ManagerWPF.Data.Models;
using Redmine.ManagerWPF.Helpers;
using System;

namespace Redmine.ManagerWPF.Data
{
    public class Context : DbContext
    {
        public Context()
        {
        }

        public Context(DbContextOptions<Context> options) : base(options)
        {
            try
            {
                if (Database.CanConnect())
                {
                    Database.Migrate();
                    Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public DbSet<Issue> Issues { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TimeInterval> TimeIntervals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Issue>()
                    .HasMany(s => s.Comments)
                    .WithOne(x => x.Issue);

            modelBuilder.Entity<Issue>()
                    .HasOne(s => s.MainTask)
                    .WithMany(s => s.Issues);

            modelBuilder.Entity<Project>()
                    .HasMany(s => s.Issues)
                    .WithOne(s => s.Project);

            modelBuilder.Entity<Issue>()
                    .HasMany(S => S.TimesForIssue)
                    .WithOne(S => S.Issue);

            modelBuilder.Entity<Comment>()
                    .HasMany(s => s.TimeForComment)
                    .WithOne(s => s.Comment);

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "";
            var databaseName = SettingsHelper.GetDatabaseName();
            var server = SettingsHelper.GetServerName();

            if (!string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(server))
            {
                connectionString = $"Server={server};Database={databaseName};Trusted_Connection=True;";
            }

            optionsBuilder.UseSqlServer(connectionString);

            base.OnConfiguring(optionsBuilder);
        }
    }
}