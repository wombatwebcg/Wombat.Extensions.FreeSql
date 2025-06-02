# Wombat.Extensions.FreeSql 用法说明

## 概述

Wombat.Extensions.FreeSql 是一个基于 FreeSql ORM 的扩展库，旨在简化 FreeSql 在 .NET 应用程序中的配置和使用。该扩展库提供了更加便捷的方式来实现数据库访问，支持仓储模式（Repository Pattern）和工作单元模式（Unit of Work Pattern），同时提供了多数据库支持和主从库配置等高级功能。

主要特性：

- 简化的 FreeSql 配置方式
- 支持通过依赖注入注册仓储和工作单元
- 支持多数据库配置
- 支持主从库配置
- 提供标准化的仓储接口和实现
- 支持事务管理
- 支持数据库操作的日志记录 

## 安装和引用

### NuGet 安装

可以通过 NuGet 包管理器安装 Wombat.Extensions.FreeSql：

```bash
Install-Package Wombat.Extensions.FreeSql
```

或者使用 .NET CLI：

```bash
dotnet add package Wombat.Extensions.FreeSql
```

### 项目引用

在项目中引用 Wombat.Extensions.FreeSql 相关命名空间：

```csharp
using Wombat.Extensions.FreeSql;
using Wombat.Extensions.FreeSql.Config;
```

### 依赖项

Wombat.Extensions.FreeSql 依赖于以下包：

- FreeSql
- FreeSql.Provider.SqlServer (或其他数据库提供程序)
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Options
- Microsoft.Extensions.Logging 

## 配置

### appsettings.json 配置

在 `appsettings.json` 文件中添加 FreeSql 的配置信息，您可以配置多个数据库连接：

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
        "DebugShowSqlPparameters": true
      },
      {
        "Key": "DB2",
        "MasterConnetion": "Data Source=server;uid=username;pwd=password;database=dbname;",
        "DataType": 1,
        "SlaveConnections": [
          {
            "ConnectionString": "Data Source=slave1;uid=username;pwd=password;database=dbname;"
          },
          {
            "ConnectionString": "Data Source=slave2;uid=username;pwd=password;database=dbname;"
          }
        ],
        "IsSyncStructure": false,
        "IsLazyLoading": true,
        "DebugShowSql": true,
        "DebugShowSqlPparameters": false,
        "CommandTimeout": 60
      }
    ]
  }
}
```

### 配置参数说明

| 参数 | 说明 | 默认值 |
|------|------|--------|
| Key | 数据库标识，用于区分不同的数据库连接 | - |
| MasterConnetion | 主数据库连接字符串 | - |
| SlaveConnections | 从数据库连接字符串列表（用于读写分离） | [] |
| DataType | 数据库类型：MySql=0, SqlServer=1, PostgreSQL=2, Oracle=3, Sqlite=4 | - |
| IsSyncStructure | 是否自动同步数据库结构 | false |
| IsLazyLoading | 是否启用延迟加载 | false |
| DebugShowSql | 是否显示执行的 SQL 语句 | true |
| DebugShowSqlPparameters | 是否显示 SQL 参数 | false |
| CommandTimeout | 命令超时时间（秒） | 30 |

### 在 Startup.cs 或 Program.cs 中注册服务

在 ASP.NET Core 应用程序中，通过 `ConfigureServices` 方法注册 FreeSql 服务：

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // 添加配置支持
    services.AddOptions();
    services.Configure<FreeSqlCollectionConfig>(Configuration.GetSection("SqlConfig"));
    
    // 注册 FreeSql 服务
    services.AddFreeSqlUnitOfWork<DB1>();  // 工作单元模式
    services.AddFreeSqlRepository<DB2>();  // 仓储模式
}
```

对于 .NET 6+ 的最小 API 模式：

```csharp
var builder = WebApplication.CreateBuilder(args);

// 添加配置支持
builder.Services.AddOptions();
builder.Services.Configure<FreeSqlCollectionConfig>(builder.Configuration.GetSection("SqlConfig"));

// 注册 FreeSql 服务
builder.Services.AddFreeSqlUnitOfWork<DB1>();  // 工作单元模式
builder.Services.AddFreeSqlRepository<DB2>();  // 仓储模式
``` 

## 仓储模式的使用

仓储模式（Repository Pattern）是一种将数据访问逻辑与业务逻辑分离的设计模式。Wombat.Extensions.FreeSql 提供了简化的仓储模式实现。

### 1. 定义实体类

首先，定义您的实体类：

```csharp
using FreeSql.DataAnnotations;

public class User
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    
    public string Username { get; set; }
    
    public string Email { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
```

### 2. 创建数据库标识接口

创建一个空接口作为数据库标识：

```csharp
// 数据库标识接口
public interface DB1
{
}
```

### 3. 定义仓储接口

```csharp
// 仓储接口定义
public interface IUserRepository : IRepositoryKey<User>
{
    // 可以添加自定义方法
    Task<User> GetByUsernameAsync(string username);
}
```

### 4. 实现仓储接口

```csharp
using Microsoft.Extensions.DependencyInjection;
using Wombat.Extensions.AutoGenerator.Attributes;
using Wombat.Extensions.FreeSql;

// 仓储实现
[AutoInject(ServiceLifetime = ServiceLifetime.Scoped)]
public class UserRepository : BaseRepository<User, DB1>, IUserRepository
{
    public UserRepository(IServiceProvider service) : base(service)
    {
    }
    
    // 实现自定义方法
    public async Task<User> GetByUsernameAsync(string username)
    {
        return await Select.Where(u => u.Username == username).FirstAsync();
    }
}
```

### 5. 注册服务

在 `Program.cs` 或 `Startup.cs` 中注册服务：

```csharp
// 注册仓储模式
services.AddFreeSqlRepository<DB1>();
```

### 6. 使用仓储

在服务或控制器中使用仓储：

```csharp
public class UserService
{
    private readonly IUserRepository _userRepository;
    
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<User> GetUserByIdAsync(int id)
    {
        return await _userRepository.Select.Where(u => u.Id == id).FirstAsync();
    }
    
    public async Task<User> CreateUserAsync(User user)
    {
        return await _userRepository.InsertAsync(user);
    }
    
    public async Task<int> UpdateUserAsync(User user)
    {
        return await _userRepository.UpdateAsync(user);
    }
    
    public async Task<int> DeleteUserAsync(int id)
    {
        return await _userRepository.DeleteAsync(u => u.Id == id);
    }
    
    // 使用自定义方法
    public async Task<User> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetByUsernameAsync(username);
    }
}
```

### 7. 事务操作

在仓储模式下进行事务操作：

```csharp
// 使用 UnitOfWork 进行事务操作
public async Task<bool> TransferMoneyAsync(int fromUserId, int toUserId, decimal amount)
{
    using (var transaction = _userRepository.UnitOfWork.GetOrBeginTransaction())
    {
        try
        {
            var fromUser = await _userRepository.Select.Where(u => u.Id == fromUserId).FirstAsync();
            var toUser = await _userRepository.Select.Where(u => u.Id == toUserId).FirstAsync();
            
            // 业务逻辑
            fromUser.Balance -= amount;
            toUser.Balance += amount;
            
            await _userRepository.UpdateAsync(fromUser);
            await _userRepository.UpdateAsync(toUser);
            
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
} 

## 工作单元模式的使用

工作单元模式（Unit of Work Pattern）是一种管理事务和数据一致性的设计模式。Wombat.Extensions.FreeSql 提供了对工作单元模式的支持。

### 1. 定义实体类

```csharp
using FreeSql.DataAnnotations;

public class Product
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public decimal Price { get; set; }
    
    public int Stock { get; set; }
}
```

### 2. 创建数据库标识接口

```csharp
// 数据库标识接口
public interface DB2
{
}
```

### 3. 定义仓储接口

```csharp
// 仓储接口定义
public interface IProductRepository : IRepositoryBase<Product, DB2>
{
    // 可以添加自定义方法
}
```

### 4. 实现仓储接口

```csharp
using Microsoft.Extensions.DependencyInjection;
using Wombat.Extensions.AutoGenerator.Attributes;
using Wombat.Extensions.FreeSql;

// 仓储实现
[AutoInject(ServiceLifetime = ServiceLifetime.Scoped)]
public class ProductRepository : RepositoryBase<Product, DB2>, IProductRepository
{
    public ProductRepository(IServiceProvider service) : base(service)
    {
    }
}
```

### 5. 注册服务

在 `Program.cs` 或 `Startup.cs` 中注册服务：

```csharp
// 注册工作单元模式
services.AddFreeSqlUnitOfWork<DB2>();
```

### 6. 使用工作单元

在服务中使用工作单元：

```csharp
public class OrderService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork<DB2> _unitOfWork;
    
    public OrderService(IProductRepository productRepository, IUnitOfWork<DB2> unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<bool> ProcessOrderAsync(int productId, int quantity)
    {
        // 开始事务
        using (var transaction = _unitOfWork.GetOrBeginTransaction())
        {
            try
            {
                // 查询产品
                var product = await _productRepository.Select.Where(p => p.Id == productId).FirstAsync();
                
                // 检查库存
                if (product.Stock < quantity)
                {
                    return false;
                }
                
                // 更新库存
                product.Stock -= quantity;
                await _productRepository.UpdateAsync(product);
                
                // 创建订单记录
                var order = new Order
                {
                    ProductId = productId,
                    Quantity = quantity,
                    TotalPrice = product.Price * quantity,
                    OrderDate = DateTime.Now
                };
                
                // 插入订单
                // 假设我们有一个 OrderRepository
                var orderRepository = _unitOfWork.GetRepository<Order>();
                await orderRepository.InsertAsync(order);
                
                // 提交事务
                transaction.Commit();
                return true;
            }
            catch
            {
                // 回滚事务
                transaction.Rollback();
                throw;
            }
        }
    }
}
```

### 7. 多仓储事务

使用工作单元模式可以很容易地管理多个仓储之间的事务：

```csharp
public class ComplexService
{
    private readonly IUnitOfWork<DB2> _unitOfWork;
    
    public ComplexService(IUnitOfWork<DB2> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<bool> ProcessComplexOperationAsync()
    {
        using (var transaction = _unitOfWork.GetOrBeginTransaction())
        {
            try
            {
                // 获取多个仓储
                var productRepository = _unitOfWork.GetRepository<Product>();
                var orderRepository = _unitOfWork.GetRepository<Order>();
                var userRepository = _unitOfWork.GetRepository<User>();
                
                // 执行多个操作
                var product = await productRepository.Select.FirstAsync();
                product.Stock -= 1;
                await productRepository.UpdateAsync(product);
                
                var order = new Order { /* ... */ };
                await orderRepository.InsertAsync(order);
                
                var user = await userRepository.Select.FirstAsync();
                user.OrderCount += 1;
                await userRepository.UpdateAsync(user);
                
                // 提交所有更改
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
```

### 8. 使用全局工作单元管理器

Wombat.Extensions.FreeSql 提供了 `IFreeSqlUnitOfWorkManager` 来管理多个工作单元：

```csharp
public class GlobalTransactionService
{
    private readonly IFreeSqlUnitOfWorkManager _unitOfWorkManager;
    
    public GlobalTransactionService(IFreeSqlUnitOfWorkManager unitOfWorkManager)
    {
        _unitOfWorkManager = unitOfWorkManager;
    }
    
    public async Task<bool> ExecuteGlobalTransactionAsync()
    {
        // 获取所有活动的工作单元
        var unitOfWorks = _unitOfWorkManager.GetUnitOfWorks();
        
        // 开始所有事务
        foreach (var uow in unitOfWorks)
        {
            uow.GetOrBeginTransaction();
        }
        
        try
        {
            // 执行多个数据库操作
            // ...
            
            // 提交所有事务
            foreach (var uow in unitOfWorks)
            {
                uow.Commit();
            }
            
            return true;
        }
        catch
        {
            // 回滚所有事务
            foreach (var uow in unitOfWorks)
            {
                uow.Rollback();
            }
            
            throw;
        }
    }
} 

## 高级功能

### 主从库配置

Wombat.Extensions.FreeSql 支持主从库配置，实现读写分离。在配置文件中可以设置一个主库和多个从库：

```json
{
  "SqlConfig": {
    "FreeSqlCollections": [
      {
        "Key": "DB1",
        "MasterConnetion": "Data Source=master;uid=username;pwd=password;database=dbname;",
        "DataType": 1,
        "SlaveConnections": [
          {
            "ConnectionString": "Data Source=slave1;uid=username;pwd=password;database=dbname;"
          },
          {
            "ConnectionString": "Data Source=slave2;uid=username;pwd=password;database=dbname;"
          }
        ]
      }
    ]
  }
}
```

配置完成后，FreeSql 会自动处理读写分离逻辑：
- 查询操作（Select）会自动路由到从库
- 插入/更新/删除操作会自动路由到主库

### 多数据库支持

Wombat.Extensions.FreeSql 支持在同一应用程序中使用多个数据库连接，每个数据库连接可以是不同的数据库类型：

```csharp
// 注册多个数据库连接
services.AddFreeSqlUnitOfWork<SqliteDB>();  // SQLite 数据库
services.AddFreeSqlUnitOfWork<SqlServerDB>();  // SQL Server 数据库
services.AddFreeSqlRepository<MySqlDB>();  // MySQL 数据库
```

### 全局过滤器

FreeSql 支持全局过滤器，可以在注册 FreeSql 服务后配置全局过滤器：

```csharp
services.AddFreeSqlUnitOfWork<DB1>();

// 获取 FreeSql 实例并配置全局过滤器
var freeSql = provider.GetRequiredService<IFreeSql<DB1>>();

// 添加软删除过滤器
freeSql.GlobalFilter.Apply<ISoftDelete>("SoftDelete", a => a.IsDeleted == false);

// 添加租户过滤器
freeSql.GlobalFilter.Apply<ITenant>("TenantFilter", a => a.TenantId == CurrentTenant.Id);
```

### SQL 监控和日志

Wombat.Extensions.FreeSql 提供了 SQL 语句的监控和日志功能，可以在配置文件中启用：

```json
{
  "SqlConfig": {
    "FreeSqlCollections": [
      {
        "Key": "DB1",
        "MasterConnetion": "...",
        "DataType": 1,
        "DebugShowSql": true,
        "DebugShowSqlPparameters": true
      }
    ]
  }
}
```

这样，所有执行的 SQL 语句及其参数都会被记录到日志中。

### JSON 数据支持

Wombat.Extensions.FreeSql 默认启用了 JSON 数据支持：

```csharp
// 在 ServiceCollectionExtensions.cs 中自动配置
freeSql.UseJsonMap();
```

这使得您可以直接将 JSON 对象存储在数据库中：

```csharp
public class Product
{
    public int Id { get; set; }
    
    // 存储为 JSON 的属性
    public Dictionary<string, object> Attributes { get; set; }
}
```

### 自动同步数据库结构

Wombat.Extensions.FreeSql 支持自动同步数据库结构，可以在配置文件中启用：

```json
{
  "SqlConfig": {
    "FreeSqlCollections": [
      {
        "Key": "DB1",
        "MasterConnetion": "...",
        "DataType": 1,
        "IsSyncStructure": true
      }
    ]
  }
}
```

当应用程序启动时，FreeSql 会自动创建/更新数据库表结构以匹配您的实体类定义。

### 延迟加载

Wombat.Extensions.FreeSql 支持延迟加载功能，可以在配置文件中启用：

```json
{
  "SqlConfig": {
    "FreeSqlCollections": [
      {
        "Key": "DB1",
        "MasterConnetion": "...",
        "DataType": 1,
        "IsLazyLoading": true
      }
    ]
  }
}
```

启用延迟加载后，导航属性会在首次访问时自动加载。 

## 完整应用示例

下面是一个使用 Wombat.Extensions.FreeSql 的完整示例，包括多数据库配置、仓储和工作单元模式的使用。

### 项目结构

```
MyApp/
├── Controllers/
│   └── ProductController.cs
├── Models/
│   ├── Product.cs
│   └── Order.cs
├── Services/
│   ├── IProductService.cs
│   └── ProductService.cs
├── Repositories/
│   ├── IProductRepository.cs
│   └── ProductRepository.cs
├── Database/
│   ├── DB1.cs
│   └── DB2.cs
├── appsettings.json
└── Program.cs
```

### 配置文件

**appsettings.json**:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "SqlConfig": {
    "FreeSqlCollections": [
      {
        "Key": "DB1",
        "MasterConnetion": "Data Source=|DataDirectory|\\Database\\ProductDb.db3;Version=3;Pooling=True;Max Pool Size=100;",
        "DataType": 4,
        "IsSyncStructure": true,
        "IsLazyLoading": false,
        "DebugShowSql": true,
        "DebugShowSqlPparameters": true
      },
      {
        "Key": "DB2",
        "MasterConnetion": "Data Source=|DataDirectory|\\Database\\OrderDb.db3;Version=3;Pooling=True;Max Pool Size=100;",
        "DataType": 4,
        "IsSyncStructure": true,
        "IsLazyLoading": false,
        "DebugShowSql": true,
        "DebugShowSqlPparameters": true
      }
    ]
  }
}
```

### 数据库标识接口

**Database/DB1.cs**:

```csharp
namespace MyApp.Database
{
    // 产品数据库标识
    public interface DB1
    {
    }
}
```

**Database/DB2.cs**:

```csharp
namespace MyApp.Database
{
    // 订单数据库标识
    public interface DB2
    {
    }
}
```

### 模型定义

**Models/Product.cs**:

```csharp
using FreeSql.DataAnnotations;
using System;

namespace MyApp.Models
{
    public class Product
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public decimal Price { get; set; }
        
        public int Stock { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
```

**Models/Order.cs**:

```csharp
using FreeSql.DataAnnotations;
using System;

namespace MyApp.Models
{
    public class Order
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        
        public int Quantity { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.Now;
    }
}
```

### 仓储定义

**Repositories/IProductRepository.cs**:

```csharp
using MyApp.Database;
using MyApp.Models;
using System.Threading.Tasks;
using Wombat.Extensions.FreeSql;

namespace MyApp.Repositories
{
    public interface IProductRepository : IRepositoryBase<Product, DB1>
    {
        Task<Product> GetByNameAsync(string name);
    }
}
```

**Repositories/ProductRepository.cs**:

```csharp
using Microsoft.Extensions.DependencyInjection;
using MyApp.Database;
using MyApp.Models;
using System;
using System.Threading.Tasks;
using Wombat.Extensions.AutoGenerator.Attributes;
using Wombat.Extensions.FreeSql;

namespace MyApp.Repositories
{
    [AutoInject(ServiceLifetime = ServiceLifetime.Scoped)]
    public class ProductRepository : RepositoryBase<Product, DB1>, IProductRepository
    {
        public ProductRepository(IServiceProvider service) : base(service)
        {
        }
        
        public async Task<Product> GetByNameAsync(string name)
        {
            return await Select.Where(p => p.Name == name).FirstAsync();
        }
    }
}
```

### 服务定义

**Services/IProductService.cs**:

```csharp
using MyApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyApp.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<int> UpdateProductAsync(Product product);
        Task<int> DeleteProductAsync(int id);
        Task<bool> ProcessOrderAsync(int productId, int quantity);
    }
}
```

**Services/ProductService.cs**:

```csharp
using MyApp.Database;
using MyApp.Models;
using MyApp.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wombat.Extensions.FreeSql;

namespace MyApp.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork<DB1> _productUow;
        private readonly IUnitOfWork<DB2> _orderUow;
        
        public ProductService(
            IProductRepository productRepository, 
            IUnitOfWork<DB1> productUow,
            IUnitOfWork<DB2> orderUow)
        {
            _productRepository = productRepository;
            _productUow = productUow;
            _orderUow = orderUow;
        }
        
        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _productRepository.Select.ToListAsync();
        }
        
        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _productRepository.Select.Where(p => p.Id == id).FirstAsync();
        }
        
        public async Task<Product> CreateProductAsync(Product product)
        {
            return await _productRepository.InsertAsync(product);
        }
        
        public async Task<int> UpdateProductAsync(Product product)
        {
            return await _productRepository.UpdateAsync(product);
        }
        
        public async Task<int> DeleteProductAsync(int id)
        {
            return await _productRepository.DeleteAsync(p => p.Id == id);
        }
        
        public async Task<bool> ProcessOrderAsync(int productId, int quantity)
        {
            // 使用跨数据库事务
            var productTran = _productUow.GetOrBeginTransaction();
            var orderTran = _orderUow.GetOrBeginTransaction();
            
            try
            {
                // 查询产品并检查库存
                var product = await _productRepository.Select.Where(p => p.Id == productId).FirstAsync();
                if (product == null || product.Stock < quantity)
                {
                    return false;
                }
                
                // 更新库存
                product.Stock -= quantity;
                await _productRepository.UpdateAsync(product);
                
                // 创建订单
                var order = new Order
                {
                    ProductId = productId,
                    Quantity = quantity,
                    TotalPrice = product.Price * quantity
                };
                
                // 使用订单仓储
                var orderRepo = _orderUow.GetRepository<Order>();
                await orderRepo.InsertAsync(order);
                
                // 提交事务
                productTran.Commit();
                orderTran.Commit();
                
                return true;
            }
            catch
            {
                // 回滚事务
                productTran.Rollback();
                orderTran.Rollback();
                throw;
            }
        }
    }
}
```

### 控制器

**Controllers/ProductController.cs**:

```csharp
using Microsoft.AspNetCore.Mvc;
using MyApp.Models;
using MyApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetAll()
        {
            return await _productService.GetAllProductsAsync();
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> Get(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            return product;
        }
        
        [HttpPost]
        public async Task<ActionResult<Product>> Create(Product product)
        {
            var createdProduct = await _productService.CreateProductAsync(product);
            return CreatedAtAction(nameof(Get), new { id = createdProduct.Id }, createdProduct);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }
            
            var result = await _productService.UpdateProductAsync(product);
            if (result == 0)
            {
                return NotFound();
            }
            
            return NoContent();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (result == 0)
            {
                return NotFound();
            }
            
            return NoContent();
        }
        
        [HttpPost("order")]
        public async Task<IActionResult> Order([FromBody] OrderRequest request)
        {
            var result = await _productService.ProcessOrderAsync(request.ProductId, request.Quantity);
            if (!result)
            {
                return BadRequest("订单处理失败，可能是库存不足");
            }
            
            return Ok("订单处理成功");
        }
    }
    
    public class OrderRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
```

### 程序入口

**Program.cs**:

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyApp.Database;
using MyApp.Services;
using Wombat.Extensions.FreeSql;
using Wombat.Extensions.FreeSql.Config;

var builder = WebApplication.CreateBuilder(args);

// 添加服务到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加 FreeSql 配置
builder.Services.AddOptions();
builder.Services.Configure<FreeSqlCollectionConfig>(builder.Configuration.GetSection("SqlConfig"));

// 注册 FreeSql 服务
builder.Services.AddFreeSqlUnitOfWork<DB1>();  // 产品数据库 - 工作单元模式
builder.Services.AddFreeSqlUnitOfWork<DB2>();  // 订单数据库 - 工作单元模式

// 注册业务服务
builder.Services.AddScoped<IProductService, ProductService>();

// 使用自动注入（如果有 Wombat.Extensions.AutoGenerator 包）
builder.Services.AddAutoInject();

var app = builder.Build();

// 配置 HTTP 请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 运行和测试

1. 启动应用程序
2. 使用 Swagger UI 或 Postman 进行测试：
   - 创建产品
   - 查询产品列表
   - 处理订单
   - 验证产品库存更新

这个示例展示了如何使用 Wombat.Extensions.FreeSql 构建一个包含两个数据库的应用程序，同时使用了仓储模式和工作单元模式来管理数据访问和事务。

## 常见问题和故障排除

### 配置问题

#### 1. 找不到对应数据库配置

**问题描述**：启动应用时抛出异常 `未找到 XXX 的 SqlConfig 配置`

**解决方案**：
- 检查 `appsettings.json` 中的 `SqlConfig.FreeSqlCollections` 配置，确保存在 `Key` 为 XXX 的配置项
- 确保配置节点名称正确，应为 `SqlConfig`
- 验证配置已正确注册：`services.Configure<FreeSqlCollectionConfig>(Configuration.GetSection("SqlConfig"))`

```csharp
// 正确的配置注册方式
services.Configure<FreeSqlCollectionConfig>(Configuration.GetSection("SqlConfig"));
```

#### 2. 数据库连接字符串格式错误

**问题描述**：启动应用时抛出数据库连接异常

**解决方案**：
- 检查连接字符串格式是否符合对应数据库类型的要求
- 确保 `DataType` 值与实际使用的数据库类型匹配
- 检查连接字符串中的路径、用户名、密码等信息是否正确

### 依赖注入问题

#### 1. 无法解析仓储服务

**问题描述**：通过依赖注入获取仓储服务时抛出异常

**解决方案**：
- 确保已正确注册对应的 FreeSql 服务：`services.AddFreeSqlRepository<DB1>()` 或 `services.AddFreeSqlUnitOfWork<DB1>()`
- 检查仓储接口和实现类是否正确定义和实现
- 如果使用 `[AutoInject]` 特性，确保已注册自动注入服务：`services.AddAutoInject()`

```csharp
// 检查仓储接口定义
public interface IUserRepository : IRepositoryKey<User> { }

// 检查仓储实现
[AutoInject(ServiceLifetime = ServiceLifetime.Scoped)]
public class UserRepository : BaseRepository<User, DB1>, IUserRepository { }
```

#### 2. 事务跨多个仓储失效

**问题描述**：在多个仓储间执行事务操作时，事务未生效

**解决方案**：
- 确保所有仓储使用同一个工作单元实例
- 使用 `IUnitOfWork<T>` 获取仓储，而不是直接注入多个独立的仓储
- 对于跨数据库事务，需要分别管理每个数据库的事务

```csharp
// 正确的多仓储事务示例
using (var transaction = _unitOfWork.GetOrBeginTransaction())
{
    var userRepo = _unitOfWork.GetRepository<User>();
    var orderRepo = _unitOfWork.GetRepository<Order>();
    
    // 执行操作...
    
    transaction.Commit();
}
```

### 数据库操作问题

#### 1. 自动同步结构失败

**问题描述**：启用了 `IsSyncStructure` 但数据库表结构未自动创建或更新

**解决方案**：
- 确保数据库用户有足够的权限创建和修改表结构
- 检查实体类的定义是否符合 FreeSql 的要求
- 验证 `IsSyncStructure` 设置为 `true`

```json
{
  "SqlConfig": {
    "FreeSqlCollections": [
      {
        "Key": "DB1",
        "IsSyncStructure": true
      }
    ]
  }
}
```

#### 2. SQL 语句不显示

**问题描述**：配置了 `DebugShowSql` 为 `true` 但看不到 SQL 日志

**解决方案**：
- 确保日志级别设置正确，FreeSql 的 SQL 日志通常在 Debug 或 Information 级别
- 检查 `DebugShowSql` 配置是否正确设置为 `true`
- 确保应用程序已正确配置日志提供程序

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "FreeSql": "Debug"
    }
  }
}
```

#### 3. 查询结果不符合预期

**问题描述**：查询结果与预期不符，缺少数据或数据不正确

**解决方案**：
- 检查全局过滤器设置，可能有隐式过滤条件
- 验证是否启用了软删除等特性，导致部分数据被过滤
- 检查实体与数据库表的映射关系是否正确

```csharp
// 检查全局过滤器
freeSql.GlobalFilter.Apply<ISoftDelete>("SoftDelete", a => a.IsDeleted == false);

// 临时禁用全局过滤器
repository.Select.DisableGlobalFilter("SoftDelete").ToList();
```

### 性能问题

#### 1. 查询性能低下

**问题描述**：数据库查询执行缓慢

**解决方案**：
- 检查生成的 SQL 语句是否高效，可能需要添加索引
- 考虑使用 AsSelect 方法优化查询
- 使用分页查询处理大量数据
- 合理设置延迟加载选项

```csharp
// 使用 AsSelect 优化查询
var query = _repository.Select.Where(x => x.Status == 1);
var optimizedQuery = query.AsSelect().Page(1, 20).ToList();

// 使用延迟加载
_repository.Select.IncludeMany(x => x.Orders).ToList();
```

#### 2. 内存占用过高

**问题描述**：应用程序内存占用过高

**解决方案**：
- 避免加载不必要的数据，使用投影查询
- 合理使用分页查询处理大数据集
- 管理缓存生命周期
- 禁用不必要的延迟加载

```csharp
// 使用投影查询减少内存占用
var result = _repository.Select
    .Where(x => x.IsActive)
    .ToList(x => new { x.Id, x.Name }); // 只选择需要的字段
```

### 其他常见问题

#### 1. 多租户实现

**问题描述**：如何实现多租户数据隔离

**解决方案**：
- 使用全局过滤器实现数据隔离
- 每个租户使用单独的数据库连接

```csharp
// 添加租户过滤器
freeSql.GlobalFilter.Apply<ITenant>("TenantFilter", a => a.TenantId == CurrentTenant.Id);
```

#### 2. 软删除实现

**问题描述**：如何实现软删除功能

**解决方案**：
- 在实体中添加 IsDeleted 属性
- 配置全局过滤器自动过滤已删除数据
- 覆盖删除方法实现软删除逻辑

```csharp
// 软删除接口
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}

// 添加软删除过滤器
freeSql.GlobalFilter.Apply<ISoftDelete>("SoftDelete", a => a.IsDeleted == false);

// 实现软删除
public async Task<int> SoftDeleteAsync(T entity)
{
    if (entity is ISoftDelete softDelete)
    {
        softDelete.IsDeleted = true;
        return await UpdateAsync(entity);
    }
    return await DeleteAsync(entity);
}
```