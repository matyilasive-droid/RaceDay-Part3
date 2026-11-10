using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public partial class RaceDayProg6212Context : DbContext
{
    public RaceDayProg6212Context()
    {
    }

    public RaceDayProg6212Context(DbContextOptions<RaceDayProg6212Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Enrolment> Enrolments { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventType> EventTypes { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VwEventCategory> VwEventCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.EventId, "IX_Categories_EventID");

            entity.HasIndex(e => new { e.EventId, e.Name }, "UQ_Categories_Event_Name").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.DistanceKm).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.EntryFee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Event).WithMany(p => p.Categories)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Categories_Event");
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Enrolments_UserID");

            entity.HasIndex(e => new { e.UserId, e.CategoryId }, "UQ_Enrolments_User_Category").IsUnique();

            entity.Property(e => e.EnrolmentId).HasColumnName("EnrolmentID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.EnrolmentDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Enrolments_EnrolmentDate");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("Registered", "DF_Enrolments_Status");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Category).WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrolments_Category");

            entity.HasOne(d => d.User).WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrolments_User");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Event");

            entity.HasIndex(e => e.EventDate, "IX_Event_EventDate");

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Event_CreatedAt");
            entity.Property(e => e.EventName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Province).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Upcoming", "DF_Event_Status");

            entity.HasOne(d => d.EventTypeNavigation).WithMany(p => p.Events)
                .HasPrincipalKey(p => p.EventTypeName)
                .HasForeignKey(d => d.EventType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Event_EventType");

            entity.HasOne(d => d.OrganisedNavigation).WithMany(p => p.Events)
                .HasForeignKey(d => d.Organised)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Event_Organiser");
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.HasIndex(e => e.EventTypeName, "UQ_EventTypes_EventTypeName").IsUnique();

            entity.Property(e => e.EventTypeId).HasColumnName("EventTypeID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EventTypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.ToTable("Result");

            entity.HasIndex(e => e.Enrolment, "UQ_Result_Enrolment").IsUnique();

            entity.Property(e => e.ResultId).HasColumnName("ResultID");
            entity.Property(e => e.FinishTime).HasPrecision(0);
            entity.Property(e => e.RecordedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Result_RecordedAt");

            entity.HasOne(d => d.EnrolmentNavigation).WithOne(p => p.Result)
                .HasForeignKey<Result>(d => d.Enrolment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Result_Enrolment");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Users_CreatedAt");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Role).HasMaxLength(30);
        });

        modelBuilder.Entity<VwEventCategory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_EventCategories");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.DistanceKm).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.EntryFee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.EventName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Province).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
