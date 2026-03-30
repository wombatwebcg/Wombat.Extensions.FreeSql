using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wombat.Extensions.FreeSql;
using Wombat.Extensions.FreeSql.Config;
using Wombat.Extensions.FreeSql.Demo;

namespace Wombat.Extensions.FreeSql.Tests;

public sealed class FreeSqlIntegrationTests : IDisposable
{
    private readonly string _dataDirectory;

    public FreeSqlIntegrationTests()
    {
        _dataDirectory = Path.Combine(Path.GetTempPath(), "WombatFreeSqlTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dataDirectory);
    }

    [Fact]
    public void Transaction_Insert_Class3_Class4_ShouldCommitAndBeQueryable()
    {
        using var host = BuildHost();
        using var scope = host.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var db1 = serviceProvider.GetRequiredService<DB1Repository>();
        var db2 = serviceProvider.GetRequiredService<DB2Repository>();
        var db3 = serviceProvider.GetRequiredService<DB3Repository>();
        var db4 = serviceProvider.GetRequiredService<IDB4Repository>();
        var freeSql = serviceProvider.GetRequiredService<IFreeSql>();

        db1.Orm.CodeFirst.SyncStructure<Class1>();
        db2.Orm.CodeFirst.SyncStructure<Class2>();
        freeSql.CodeFirst.SyncStructure<Class3>();
        freeSql.CodeFirst.SyncStructure<Class4>();

        var class1Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var class2Id = class1Id + 1;
        db1.Insert(new Class1 { Id = class1Id });
        db2.Insert(new Class2 { Id = class2Id });

        var class3Id = class1Id + 2;
        var class4Id = class1Id + 3;
        var class3 = new Class3
        {
            Id = class3Id,
            Properties = new Dictionary<long, string> { [1] = "2" }
        };
        var class4 = new Class4 { Id = class4Id };

        using (var dbTransaction = freeSql.CreateUnitOfWork())
        {
            dbTransaction.GetRepository<Class3>().Insert(class3);
            dbTransaction.GetRepository<Class4>().Insert(class4);
            dbTransaction.Commit();
        }

        var db1Entity = db1.Select.Where(p => p.Id == class1Id).First();
        var db2Entity = db2.Select.Where(p => p.Id == class2Id).First();
        var class3Entity = db3.Select.Where(p => p.Id == class3Id).First();
        var class4Entity = db4.Select.Where(p => p.Id == class4Id).First();

        Assert.NotNull(db1Entity);
        Assert.NotNull(db2Entity);
        Assert.NotNull(class3Entity);
        Assert.NotNull(class4Entity);
        Assert.NotNull(class3Entity.Properties);
        Assert.True(class3Entity.Properties.TryGetValue(1, out var value));
        Assert.Equal("2", value);
    }

    [Fact]
    public void Transaction_WhenFailed_ShouldRollback()
    {
        using var host = BuildHost();
        using var scope = host.Services.CreateScope();
        var freeSql = scope.ServiceProvider.GetRequiredService<IFreeSql>();
        var db3 = scope.ServiceProvider.GetRequiredService<DB3Repository>();
        var db4 = scope.ServiceProvider.GetRequiredService<IDB4Repository>();

        freeSql.CodeFirst.SyncStructure<Class3>();
        freeSql.CodeFirst.SyncStructure<Class4>();

        var class3Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var duplicateClass4Id = class3Id + 1;

        Assert.ThrowsAny<Exception>(() =>
        {
            using var dbTransaction = freeSql.CreateUnitOfWork();
            dbTransaction.GetRepository<Class3>().Insert(new Class3
            {
                Id = class3Id,
                Properties = new Dictionary<long, string> { [1] = "rollback" }
            });
            dbTransaction.GetRepository<Class4>().Insert(new Class4 { Id = duplicateClass4Id });
            dbTransaction.GetRepository<Class4>().Insert(new Class4 { Id = duplicateClass4Id });
            dbTransaction.Commit();
        });

        var class3Entity = db3.Select.Where(p => p.Id == class3Id).First();
        var class4Entity = db4.Select.Where(p => p.Id == duplicateClass4Id).First();
        Assert.Null(class3Entity);
        Assert.Null(class4Entity);
    }

    private IHost BuildHost()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddOptions();
        builder.Services.Configure<FreeSqlCollectionConfig>(options =>
        {
            options.FreeSqlCollections =
            [
                new FreeSqlConfig
                {
                    Key = nameof(DB1),
                    MasterConnetion = BuildSqliteConnectionString("DB1.db3"),
                    DataType = DataType.Sqlite,
                    IsSyncStructure = true,
                    IsLazyLoading = false,
                    DebugShowSql = false,
                    DebugShowSqlPparameters = false,
                    CommandTimeout = 30
                },
                new FreeSqlConfig
                {
                    Key = nameof(DB2),
                    MasterConnetion = BuildSqliteConnectionString("DB2.db3"),
                    DataType = DataType.Sqlite,
                    IsSyncStructure = true,
                    IsLazyLoading = false,
                    DebugShowSql = false,
                    DebugShowSqlPparameters = false,
                    CommandTimeout = 30
                },
                new FreeSqlConfig
                {
                    Key = nameof(DB3),
                    MasterConnetion = BuildSqliteConnectionString("DB3.db3"),
                    DataType = DataType.Sqlite,
                    IsSyncStructure = true,
                    IsLazyLoading = false,
                    DebugShowSql = false,
                    DebugShowSqlPparameters = false,
                    CommandTimeout = 30
                }
            ];
        });

        builder.Services.AddFreeSqlUnitOfWork<DB1>();
        builder.Services.AddFreeSqlUnitOfWork<DB2>();
        builder.Services.AddFreeSqlRepository<DB3>();
        builder.Services.AddScoped<DB1Repository>();
        builder.Services.AddScoped<DB2Repository>();
        builder.Services.AddScoped<DB3Repository>();
        builder.Services.AddScoped<IDB4Repository, DB4Repository>();

        return builder.Build();
    }

    private string BuildSqliteConnectionString(string fileName)
    {
        var dbPath = Path.Combine(_dataDirectory, fileName);
        return $"Data Source={dbPath};Version=3;Pooling=True;Max Pool Size=10;";
    }

    public void Dispose()
    {
        if (Directory.Exists(_dataDirectory))
        {
            Directory.Delete(_dataDirectory, true);
        }
    }
}
