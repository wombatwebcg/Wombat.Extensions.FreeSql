using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wombat.Extensions.FreeSql.Config;
using Wombat.Extensions.FreeSql;
using FreeSql;
using System.Data.Common;

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
            var db4 = app.Services.GetService<IDB4Repository>();

            //var config = builder.Configuration.GetSection("SqlConfig");


            var unitOfWork = app.Services.GetService<IUnitOfWork>();
            var test11 = db1.Select.First(p => p.Id == 103);

            //using (var transaction = db1.UnitOfWork.GetOrBeginTransaction())
            //{
            //    db1.Insert(new Class1() { Id = 1100 });
            //}
            //var test12 = db2.Select.First(p => p.Id == 103);

            //using (var transaction = db2.UnitOfWork.GetOrBeginTransaction())
            //{
            //    db2.Insert(new Class2() { Id = 103 });
            //    transaction.Commit();

            //}

            //db4.Insert(new Class4() { Id = 101445889 });


            using (var dbTransaction = app.Services.GetService<IFreeSql>().CreateUnitOfWork())
            {
                var c3 = new Class3() { Id = 123456, Properties = new Dictionary<long, string>() };
                c3.Properties.Add(1, "2");
                dbTransaction.GetRepository<Class3>().Insert(c3);
                dbTransaction.GetRepository<Class4>().Insert(new Class4() { Id = 1014457109 });
                dbTransaction.Commit();
            }


            //    var test5 = db3.Select.First(p => p.Id == 101);

            //var test6 = db4.Select.First(p => p.Id == 101);

            //var c3 = new Class3() { Id = 123, Properties = new Dictionary<long, string>() };
            //c3.Properties.Add(1, "2");
            //db3.Insert(c3);
            ////db3.UnitOfWork.Commit();
            //db4.Insert(new Class4() { Id = 1014457 });


            //db4.Orm.Insert(new Class4() { Id = 108 });
            //db4.UnitOfWork.Commit();

            var test1 = db1.Select.First(p => p.Id == 103);

            var test2 = db2.Select.First(p => p.Id == 103);

            var test3 = db3.Select.First(p => p.Id == 101);

        }
    }
}
