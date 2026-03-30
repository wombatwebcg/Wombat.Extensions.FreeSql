using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wombat.Extensions.FreeSql.Config;
using Wombat.Extensions.FreeSql;
using FreeSql;

namespace Wombat.Extensions.FreeSql.Demo
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddOptions();
            builder.Services.Configure<FreeSqlCollectionConfig>(builder.Configuration.GetSection("SqlConfig"));

            builder.Services.AddFreeSqlUnitOfWork<DB1>();
            builder.Services.AddFreeSqlUnitOfWork<DB2>();
            builder.Services.AddFreeSqlRepository<DB3>();

            builder.Services.AddAutoInject();
            var app = builder.Build();

            app.Start();
            using var scope = app.Services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var db1 = serviceProvider.GetRequiredService<DB1Repository>();
            var db2 = serviceProvider.GetRequiredService<DB2Repository>();
            var db3 = serviceProvider.GetRequiredService<DB3Repository>();
            var db4 = serviceProvider.GetRequiredService<IDB4Repository>();
            var freeSql = serviceProvider.GetRequiredService<IFreeSql>();
            freeSql.CodeFirst.SyncStructure<Class3>();
            freeSql.CodeFirst.SyncStructure<Class4>();
            var class3Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var class4Id = class3Id + 1;
            var class3 = new Class3 { Id = class3Id, Properties = new Dictionary<long, string> { [1] = "2" } };
            var class4 = new Class4 { Id = class4Id };

            using (var dbTransaction = freeSql.CreateUnitOfWork())
            {
                try
                {
                    dbTransaction.GetRepository<Class3>().Insert(class3);
                    dbTransaction.GetRepository<Class4>().Insert(class4);
                    dbTransaction.Commit();
                }
                catch
                {
                    dbTransaction.Rollback();
                    throw;
                }
            }

            var db1Count = db1.Select.Count();
            var db2Count = db2.Select.Count();
            var test3 = db3.Select.Where(p => p.Id == class3Id).First();
            var test4 = db4.Select.Where(p => p.Id == class4Id).First();
            if (db1Count < 0 || db2Count < 0 || test3 == null || test4 == null)
            {
                throw new InvalidOperationException("测试失败：查询结果不完整。");
            }
            if (test3.Properties == null || !test3.Properties.TryGetValue(1, out var value) || value != "2")
            {
                throw new InvalidOperationException("测试失败：Class3.Properties 持久化不正确。");
            }
            return 0;
        }
    }
}
