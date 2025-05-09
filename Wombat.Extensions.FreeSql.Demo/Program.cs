using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wombat.Extensions.FreeSql.Config;
using Wombat.Extensions.FreeSql;

namespace Wombat.Extensions.FreeSql.Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
			var builder = Host.CreateApplicationBuilder(args);
			builder.Services.AddOptions();
			builder.Services.Configure<FreeSqlCollectionConfig>(builder.Configuration.GetSection("SqlConfig"));

            builder.Services.AddFreeSql<DB1>();
			builder.Services.AddFreeSql<DB2>();
			builder.Services.AddAutoInject();
			var app = builder.Build();
           
            app.Start();
            var db1 = app.Services.GetService<DB1Repository>();
            var db2 = app.Services.GetService<DB2Repository>();
            var config = builder.Configuration.GetSection("SqlConfig");
            db1.Insert(new Class1() { Id = 103 });
            db2.Insert(new Class2() { Id = 103 });

           var test1 = db1.Select.First(p => p.Id == 103);

           var test2 = db2.Select.First(p => p.Id == 103);
        }
    }
}
