# Wombat.Extensions.FreeSql

`Wombat.Extensions.FreeSql` 是一个基于 `FreeSql` 的扩展库，用于在 .NET 项目中快速接入多数据库配置、仓储模式和工作单元模式，并与 `Microsoft.Extensions.DependencyInjection` / `Options` 配合使用。

仓库内同时提供了：

- 核心库：`Wombat.Extensions.FreeSql`
- 控制台示例：`Wombat.Extensions.FreeSql.Demo`
- 集成测试：`Wombat.Extensions.FreeSql.Tests`

## 功能特性

- 基于配置自动创建 FreeSql 实例
- 支持多个数据库连接
- 支持主从库配置
- 支持仓储模式
- 支持工作单元模式
- 自动启用 `JsonMap`
- 支持 SQL 日志输出与命令超时配置
- 支持自动同步表结构和延迟加载

## 目标框架

当前核心库目标框架：

- `netstandard2.0`

## 安装

安装核心包：

```bash
dotnet add package Wombat.Extensions.FreeSql
```

按实际数据库类型安装对应的 FreeSql Provider，例如：

```bash
dotnet add package FreeSql.Provider.Sqlite
dotnet add package FreeSql.Provider.SqlServer
dotnet add package FreeSql.Provider.PostgreSQL
```

如果你希望像示例项目一样使用特性自动注入仓储，还可以额外安装：

```bash
dotnet add package Wombat.Extensions.AutoGenerator
dotnet add package Wombat.Extensions.AutoGenerator.Attributes
```

## 配置说明

在 `appsettings.json` 中配置 `SqlConfig.FreeSqlCollections`：

```json
{
  "SqlConfig": {
    "FreeSqlCollections": [
      {
        "Key": "DB1",
        "MasterConnetion": "Data Source=|DataDirectory|\\Database\\DB1.db3;Version=3;Pooling=True;Max Pool Size=100;",
        "DataType": 4,
        "IsSyncStructure": true,
        "IsLazyLoading": false,
        "DebugShowSql": true,
        "DebugShowSqlPparameters": true,
        "CommandTimeout": 30,
        "SlaveConnections": []
      }
    ]
  }
}
```

### 配置字段

| 字段 | 说明 |
| --- | --- |
| `Key` | 数据库标识，必须和注册时使用的上下文标记类型名称一致 |
| `MasterConnetion` | 主库连接字符串 |
| `SlaveConnections` | 从库连接列表，用于读写分离 |
| `DataType` | `FreeSql.DataType` 枚举值 |
| `IsSyncStructure` | 是否自动同步表结构 |
| `IsLazyLoading` | 是否启用延迟加载 |
| `DebugShowSql` | 是否输出 SQL |
| `DebugShowSqlPparameters` | 是否输出 SQL 参数 |
| `CommandTimeout` | 命令超时时间，单位秒 |

### 关键约束

- `Key` 必须等于标记接口名称，例如 `AddFreeSqlUnitOfWork<DB1>()` 对应的配置项必须是 `"Key": "DB1"`。
- `AddFreeSqlRepository<T>()` 在找不到对应配置时会抛出异常。
- 库内部默认会调用 `UseJsonMap()`。

## 快速开始

### 1. 定义数据库标记接口

```csharp
public interface DB1
{
}

public interface DB2
{
}
```

### 2. 注册配置与服务

```csharp
using Microsoft.Extensions.DependencyInjection;
using Wombat.Extensions.FreeSql;
using Wombat.Extensions.FreeSql.Config;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions();
builder.Services.Configure<FreeSqlCollectionConfig>(
    builder.Configuration.GetSection("SqlConfig"));

// 工作单元模式
builder.Services.AddFreeSqlUnitOfWork<DB1>();

// 普通仓储模式
builder.Services.AddFreeSqlRepository<DB2>();
```

## 两种使用方式

### 工作单元模式

适合需要多个仓储共享同一事务的场景。

注册：

```csharp
builder.Services.AddFreeSqlUnitOfWork<DB1>();
```

注册后可使用：

- `IFreeSql<DB1>`
- `IUnitOfWork<DB1>`
- 基于 `RepositoryBase<TEntity, DB1>` 的仓储

仓储接口与实现示例：

```csharp
using Microsoft.Extensions.DependencyInjection;
using Wombat.Extensions.AutoGenerator.Attributes;
using Wombat.Extensions.FreeSql;

public class User
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public interface IUserRepository : IRepositoryBase<User, DB1>
{
}

[AutoInject(ServiceLifetime = ServiceLifetime.Scoped)]
public class UserRepository : RepositoryBase<User, DB1>, IUserRepository
{
    public UserRepository(IServiceProvider service) : base(service)
    {
    }
}
```

服务中使用工作单元：

```csharp
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork<DB1> _unitOfWork;

    public UserService(IUserRepository userRepository, IUnitOfWork<DB1> unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateAsync(User user)
    {
        await _userRepository.UowInsertAsync(user);
        _unitOfWork.Commit();
    }
}
```

`IRepositoryBase<TEntity, TContext>` 额外提供了这组带事务的 CUD 方法：

- `UowInsert` / `UowInsertAsync`
- `UowUpdate` / `UowUpdateAsync`
- `UowDelete` / `UowDeleteAsync`

### 普通仓储模式

适合单库、轻量使用场景。

注册：

```csharp
builder.Services.AddFreeSqlRepository<DB2>();
```

注册后可使用：

- `IFreeSql`
- 基于 `BaseRepository<TEntity, DB2>` 的仓储

仓储接口与实现示例：

```csharp
using Microsoft.Extensions.DependencyInjection;
using Wombat.Extensions.AutoGenerator.Attributes;
using Wombat.Extensions.FreeSql;

public class Product
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public interface IProductRepository : IRepositoryKey<Product>
{
}

[AutoInject(typeof(IProductRepository), ServiceLifetime = ServiceLifetime.Scoped)]
public class ProductRepository : BaseRepository<Product, DB2>, IProductRepository
{
    public ProductRepository(IServiceProvider service) : base(service)
    {
    }
}
```

使用示例：

```csharp
public class ProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<Product?> GetAsync(long id)
    {
        return _productRepository.Select.Where(x => x.Id == id).FirstAsync();
    }

    public Task<Product> CreateAsync(Product entity)
    {
        return _productRepository.InsertAsync(entity);
    }
}
```

## 事务说明

### 单库事务

在工作单元模式下，可以直接使用 `IUnitOfWork<TContext>`：

```csharp
using (var tran = _unitOfWork.GetOrBeginTransaction())
{
    try
    {
        await _userRepository.UowInsertAsync(user);
        tran.Commit();
    }
    catch
    {
        tran.Rollback();
        throw;
    }
}
```

### 通过 FreeSql 原生工作单元处理

示例项目和测试中也演示了直接使用 `IFreeSql.CreateUnitOfWork()`：

```csharp
using var scope = host.Services.CreateScope();
var freeSql = scope.ServiceProvider.GetRequiredService<IFreeSql>();

using var dbTransaction = freeSql.CreateUnitOfWork();
dbTransaction.GetRepository<Class3>().Insert(new Class3 { Id = 1 });
dbTransaction.GetRepository<Class4>().Insert(new Class4 { Id = 2 });
dbTransaction.Commit();
```

## 日志与调试

启用以下配置后，库会通过 `ILogger<IFreeSql>` 输出 SQL：

```json
{
  "SqlConfig": {
    "FreeSqlCollections": [
      {
        "Key": "DB1",
        "DebugShowSql": true,
        "DebugShowSqlPparameters": true
      }
    ]
  }
}
```

## 已验证的用法来源

当前 README 内容主要依据以下项目整理：

- `Wombat.Extensions.FreeSql` 核心源码
- `Wombat.Extensions.FreeSql.Demo` 示例程序
- `Wombat.Extensions.FreeSql.Tests` 集成测试

如果你需要更长篇的中文说明，可以继续参考仓库中的 `Wombat.Extensions.FreeSql用法说明.md`；建议将本 `README.md` 作为 GitHub 首页文档，长文档作为补充说明。
