using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var optionsBuilder = new DbContextOptionsBuilder<LibraryContext>();
var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = "library.db",
    Password = "my-password",
}.ToString();
optionsBuilder.UseSqlite(connectionString);

using var context = new LibraryContext(optionsBuilder.Options);

context.Database.EnsureCreated();

public class LibraryContext(DbContextOptions<LibraryContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; init; }
}

public class Book
{
    public int Id { get; init; }

    [StringLength(100)]
    public required string Title { get; init; }
}
