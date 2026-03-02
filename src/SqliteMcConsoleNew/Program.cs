using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

// 核心：在任何数据库操作之前拦截并重定向驱动
raw.SetProvider(new SQLite3Provider_e_sqlite3mc());

var optionsBuilder = new DbContextOptionsBuilder<LibraryContext>();
var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = "library.db",
    Password = "my-password", // SQLite3MC 会自动处理这个密钥
}.ToString();

optionsBuilder.UseSqlite(connectionString);

using var context = new LibraryContext(optionsBuilder.Options);

// 这里会创建数据库并应用加密密钥
context.Database.EnsureCreated();

await context.Books.AddAsync(new Book() { Title = Guid.NewGuid().ToString() });

await context.SaveChangesAsync();

Console.WriteLine(await context.Books.CountAsync());

// --- 模型定义保持不变 ---

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
