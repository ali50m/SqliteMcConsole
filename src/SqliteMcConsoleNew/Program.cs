using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

// 1. 显式指定加密驱动
raw.SetProvider(new SQLite3Provider_e_sqlite3mc());

// 2. 标准连接字符串（不包含非法关键字）
var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = "library.db",
    Password = "my-password",
}.ToString();

var optionsBuilder = new DbContextOptionsBuilder<LibraryContext>();
optionsBuilder.UseSqlite(connectionString);

await using var context = new LibraryContext(optionsBuilder.Options);

// 3. 在操作之前，手动注入加密协议 (锁定为 chacha20)
await using var conn = context.Database.GetDbConnection();
await conn.OpenAsync();
await using (var cmd = conn.CreateCommand())
{
    // 这行指令会告诉 SQLite3MC 接下来使用 chacha20 协议
    cmd.CommandText = "PRAGMA cipher = 'chacha20';";
    await cmd.ExecuteNonQueryAsync();
}

// 4. 现在可以安全地执行 EF 操作了
await context.Database.EnsureCreatedAsync();

await context.Books.AddAsync(new Book { Title = Guid.NewGuid().ToString() });
await context.SaveChangesAsync();

Console.WriteLine($"目前书籍总数: {await context.Books.CountAsync()}");

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
