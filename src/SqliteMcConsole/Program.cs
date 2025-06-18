using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var optionsBuilder = new DbContextOptionsBuilder<LibraryContext>();

var connectionString = CreateEncryptedConnectionString();
optionsBuilder.UseSqlite(connectionString);

using var context = new LibraryContext(optionsBuilder.Options);

context.Database.EnsureCreated();

context.Books.Add(new Book { Title = DateTimeOffset.UtcNow.ToString("G") });
context.SaveChanges();
return;

string CreateEncryptedConnectionString()
{
    const string password = "my-password";

    using var connection = new SqliteConnection(
        new SqliteConnectionStringBuilder { DataSource = "library.db" }.ToString()
    );

    connection.Open();

    using var command = connection.CreateCommand();
    command.CommandText = $"PRAGMA key = '{password}';";
    command.ExecuteNonQuery();

    connection.Close();

    return connection.ConnectionString;
}

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
