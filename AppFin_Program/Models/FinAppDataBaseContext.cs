using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AppFin_Program.Models;

public partial class FinAppDataBaseContext : DbContext
{
    public FinAppDataBaseContext()
    {
    }

    public FinAppDataBaseContext(DbContextOptions<FinAppDataBaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Budget> Budgets { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Goal> Goals { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionCategory> TransactionCategories { get; set; }

    public virtual DbSet<TransactionType> TransactionTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=FinApp_DataBase;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Budgets_UserId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.Budgets).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Goals_UserId");

            entity.Property(e => e.TargetAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.User).WithMany(p => p.Goals).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Reports_UserId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Expense).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Income).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.Reports).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {

            entity.HasIndex(e => e.TransactionCategoriesId, "IX_Transactions_TransactionTypeId");
            entity.HasIndex(e => e.UserId, "IX_Transactions_UserId");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.TransactionCategories).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.TransactionCategoriesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_TransactionCategories");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_Users");
        });

        modelBuilder.Entity<TransactionCategory>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasIndex(e => e.CategoryId, "IX_TransactionCategories_CategoryId");
            entity.HasIndex(e => e.TransactionTypeId, "IX_TransactionCategories_TransactionId");

            entity.HasOne(d => d.Category).WithMany(p => p.TransactionCategories)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TransactionCategories_Categories");
            entity.HasOne(d => d.TransactionType).WithMany(p => p.TransactionCategories)
                .HasForeignKey(d => d.TransactionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TransactionCategories_TransactionTypes");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
