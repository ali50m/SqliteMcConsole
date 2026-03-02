using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

// 1. 初始化：使用 utelle 官方的 Batteries
// 这行代码会自动寻找并加载项目中的 SQLite3MC 原生库
Batteries.Init();

var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = "library.db",
    Password = "my-password",
}.ToString();

var optionsBuilder = new DbContextOptionsBuilder<LibraryContext>();
optionsBuilder.UseSqlite(connectionString);

await using var context = new LibraryContext(optionsBuilder.Options);

// 2. 创建数据库（如果不存在，会按默认加密创建）
await context.Database.EnsureCreatedAsync();

// 3. 验证驱动信息
var conn = (SqliteConnection)context.Database.GetDbConnection();
await conn.OpenAsync();
using (var cmd = conn.CreateCommand())
{
    // 强制锁定协议（为了兼容你的历史程序）
    cmd.CommandText = "PRAGMA cipher = 'chacha20';";
    await cmd.ExecuteNonQueryAsync();

    // 查询版本，确认是否真的是 utelle 的引擎
    cmd.CommandText = "SELECT sqlite3mc_version();";
    var version = await cmd.ExecuteScalarAsync();

    Console.WriteLine($"--- 官方维护版验证 ---");
    Console.WriteLine($"加密引擎版本: {version}");
}

// 4. 业务逻辑
await context.Books.AddAsync(new Book { Title = "测试官方维护版" });
await context.SaveChangesAsync();

Console.WriteLine($"目前总数: {await context.Books.CountAsync()}");

// --- 模型定义 (Book, LibraryContext) 保持不变 ---
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
