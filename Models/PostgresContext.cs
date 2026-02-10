using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Skoob.Models;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Mainuser> Mainusers { get; set; }

    public virtual DbSet<Statusbook> Statusbooks { get; set; }

    public virtual DbSet<Userbook> Userbooks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("author_pkey");

            entity.ToTable("author", "skoob");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Birthdate).HasColumnName("birthdate");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("books_pkey");

            entity.ToTable("books", "skoob");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.PagesNumber).HasColumnName("pages_number");
            entity.Property(e => e.PublishingYear).HasColumnName("publishing_year");
            entity.Property(e => e.Synopsis)
                .HasMaxLength(500)
                .HasColumnName("synopsis");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");

            entity.HasOne(d => d.Author).WithMany(p => p.Books)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_book_author");

            entity.HasMany(d => d.Genres).WithMany(p => p.Books)
                .UsingEntity<Dictionary<string, object>>(
                    "Booksgenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("fk_books_genres_genre"),
                    l => l.HasOne<Book>().WithMany()
                        .HasForeignKey("BookId")
                        .HasConstraintName("fk_books_genres_book"),
                    j =>
                    {
                        j.HasKey("BookId", "GenreId").HasName("pk_books_genres");
                        j.ToTable("booksgenres", "skoob");
                        j.IndexerProperty<Guid>("BookId").HasColumnName("book_id");
                        j.IndexerProperty<Guid>("GenreId").HasColumnName("genre_id");
                    });
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("genre_pkey");

            entity.ToTable("genre", "skoob");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Mainuser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mainuser_pkey");

            entity.ToTable("mainuser", "skoob");

            entity.HasIndex(e => e.Email, "mainuser_email_key").IsUnique();

            entity.HasIndex(e => e.UserName, "mainuser_user_name_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.UserName)
                .HasMaxLength(15)
                .HasColumnName("user_name");
            entity.Property(e => e.UserPassword)
                .HasMaxLength(100)
                .HasColumnName("user_password");
        });

        modelBuilder.Entity<Statusbook>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("statusbooks_pkey");

            entity.ToTable("statusbooks", "skoob");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
        });

        modelBuilder.Entity<Userbook>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.BookId }).HasName("pk_userbooks");

            entity.ToTable("userbooks", "skoob", t =>
            {
                t.HasCheckConstraint("CK_UserBooks_Rating", "rating BETWEEN 1 AND 5");
            });

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.FinishDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("finish_date");
            entity.Property(e => e.PagesRead)
                .HasDefaultValue(0)
                .HasColumnName("pages_read");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Review)
                .HasMaxLength(500)
                .HasColumnName("review");
            entity.Property(e => e.StartDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_date");
            entity.Property(e => e.StatusId).HasColumnName("status_id");

            entity.HasOne(d => d.Book).WithMany(p => p.Userbooks)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_user_books_book");

            entity.HasOne(d => d.Status).WithMany(p => p.Userbooks)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user_books_status");

            entity.HasOne(d => d.User).WithMany(p => p.Userbooks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_user_books_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

}
