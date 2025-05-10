using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wombat.Extensions.FreeSql.Config;
using Wombat.Extensions.FreeSql;
using FreeSql;

namespace Wombat.Extensions.FreeSql.Demo
{
    internal class Program
    {
        static void Main(string[] args)
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
            var db1 = app.Services.GetService<DB1Repository>();
            var db2 = app.Services.GetService<DB2Repository>();
            var db3 = app.Services.GetService<DB3Repository>();
            //var config = builder.Configuration.GetSection("SqlConfig");


            var unitOfWork = app.Services.GetService<UnitOfWork>();


            db1.Insert(new Class1() { Id = 103 });
            db2.Insert(new Class2() { Id = 104 });
            db3.Insert(new Class3() { Id = 105 });


            var test1 = db1.Select.First(p => p.Id == 103);

            var test2 = db2.Select.First(p => p.Id == 103);

            var test3 = db3.Select.First(p => p.Id == 101);

        }
    }
}
