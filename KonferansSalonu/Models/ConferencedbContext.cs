using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace KonferansSalonu.Models;

public partial class ConferencedbContext : DbContext
{
    public ConferencedbContext()
    {
    }

    public ConferencedbContext(DbContextOptions<ConferencedbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auditlog> Auditlogs { get; set; }

    public virtual DbSet<Culture> Cultures { get; set; }

    public virtual DbSet<Languagekey> Languagekeys { get; set; }

    public virtual DbSet<Passwordresettoken> Passwordresettokens { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<Seatgroup> Seatgroups { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<Translation> Translations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=conferencedb;user=root;password=794613", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.41-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Auditlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("auditlogs")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .HasColumnName("action");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("createdat");
            entity.Property(e => e.Details)
                .HasColumnType("text")
                .HasColumnName("details");
            entity.Property(e => e.Userid).HasColumnName("userid");
        });

        modelBuilder.Entity<Culture>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("cultures")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Code, "code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(10)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Languagekey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("languagekeys")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Keyname, "keyname").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Keyname)
                .HasMaxLength(100)
                .HasColumnName("keyname");
        });

        modelBuilder.Entity<Passwordresettoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("passwordresettoken")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Userid, "userid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("createdat");
            entity.Property(e => e.Expirationdate)
                .HasColumnType("datetime")
                .HasColumnName("expirationdate");
            entity.Property(e => e.Isused).HasColumnName("isused");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .HasColumnName("token");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Passwordresettokens)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("passwordresettoken_ibfk_1");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("rooms")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Capacity)
                .HasDefaultValueSql("'0'")
                .HasColumnName("capacity");
            entity.Property(e => e.Deletedat)
                .HasColumnType("datetime")
                .HasColumnName("deletedat");
            entity.Property(e => e.Isdisabled).HasColumnName("isdisabled");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("seats")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Seatgroupid, "seats_ibfk_1");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Defaultheight)
                .HasDefaultValueSql("'40'")
                .HasColumnName("defaultheight");
            entity.Property(e => e.Defaultwidth)
                .HasDefaultValueSql("'40'")
                .HasColumnName("defaultwidth");
            entity.Property(e => e.Height)
                .HasDefaultValueSql("'0'")
                .HasColumnName("height");
            entity.Property(e => e.Isresize)
                .HasDefaultValueSql("'0'")
                .HasColumnName("isresize");
            entity.Property(e => e.Label)
                .HasMaxLength(20)
                .HasColumnName("label");
            entity.Property(e => e.Rotation).HasColumnName("rotation");
            entity.Property(e => e.Scalepercentage)
                .HasDefaultValueSql("'0'")
                .HasColumnName("scalepercentage");
            entity.Property(e => e.Seatgroupid).HasColumnName("seatgroupid");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'0'")
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasDefaultValueSql("'1'")
                .HasComment("1-armchair, 2-chair, 3-table, 4-rectangletable, 5-stage")
                .HasColumnName("type");
            entity.Property(e => e.Width)
                .HasDefaultValueSql("'0'")
                .HasColumnName("width");
            entity.Property(e => e.X).HasColumnName("x");
            entity.Property(e => e.Y).HasColumnName("y");

            entity.HasOne(d => d.Seatgroup).WithMany(p => p.Seats)
                .HasForeignKey(d => d.Seatgroupid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("seats_ibfk_1");
        });

        modelBuilder.Entity<Seatgroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("seatgroups")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Sectionid, "sectionid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Sectionid).HasColumnName("sectionid");

            entity.HasOne(d => d.Section).WithMany(p => p.Seatgroups)
                .HasForeignKey(d => d.Sectionid)
                .HasConstraintName("seatgroups_ibfk_1");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("section")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Roomid, "roomid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .HasDefaultValueSql("'#FFFFFF'")
                .HasColumnName("color");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Roomid).HasColumnName("roomid");

            entity.HasOne(d => d.Room).WithMany(p => p.Sections)
                .HasForeignKey(d => d.Roomid)
                .HasConstraintName("section_ibfk_1");
        });

        modelBuilder.Entity<Translation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("translations")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => new { e.Cultureid, e.Keyid }, "UQ_Culture_Key").IsUnique();

            entity.HasIndex(e => e.Keyid, "keyid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cultureid).HasColumnName("cultureid");
            entity.Property(e => e.Keyid).HasColumnName("keyid");
            entity.Property(e => e.Value)
                .HasColumnType("text")
                .HasColumnName("value");

            entity.HasOne(d => d.Culture).WithMany(p => p.Translations)
                .HasForeignKey(d => d.Cultureid)
                .HasConstraintName("translations_ibfk_1");

            entity.HasOne(d => d.Key).WithMany(p => p.Translations)
                .HasForeignKey(d => d.Keyid)
                .HasConstraintName("translations_ibfk_2");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("users")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Cultureid, "cultureid");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cultureid)
                .HasDefaultValueSql("'1'")
                .HasColumnName("cultureid");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .HasColumnName("fullname");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValueSql("'user'")
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Culture).WithMany(p => p.Users)
                .HasForeignKey(d => d.Cultureid)
                .HasConstraintName("users_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
