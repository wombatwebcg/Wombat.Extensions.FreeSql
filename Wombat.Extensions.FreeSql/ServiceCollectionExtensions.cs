using System;
using FreeSql;
using FreeSql.Aop;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wombat.Extensions.FreeSql.Config;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Microsoft.Extensions.Configuration;



namespace Wombat.Extensions.FreeSql
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 事务模式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        public static void AddFreeSqlUnitOfWork<T>(this IServiceCollection service)
        {
            //var thisFontColor = (int)Console.ForegroundColor;
            //var newFontColor = (ConsoleColor)((thisFontColor + 1) > 15 ? 0 : (thisFontColor + 1));

            FreeSqlConfig config = new FreeSqlConfig();
            service.TryAddScoped<IFreeSqlUnitOfWorkManager, FreeSqlUnitOfWorkManager>();

            service.AddSingleton(f =>
            {
                var log = f.GetRequiredService<ILogger<IFreeSql>>();
                config = f.GetRequiredService<IOptions<FreeSqlCollectionConfig>>().Value.FreeSqlCollections.FirstOrDefault(x => x.Key == typeof(T).Name);
                var builder = new FreeSqlBuilder()
                    .UseConnectionString(config.DataType, config.MasterConnetion)
                    .UseAutoSyncStructure(config.IsSyncStructure)
                    .UseMonitorCommand(executing =>
                    {
                        executing.CommandTimeout = config.CommandTimeout;

                        if (config.DebugShowSql)
                        {
                            //Console.ForegroundColor = newFontColor;
                            //Console.WriteLine("\n=================================================================================\n");
                            //Console.WriteLine(executed.CommandText + "\n");

                            string parametersValue = "";
                            if (config.DebugShowSqlPparameters)
                            {
                                for (int i = 0; i < executing.Parameters.Count; i++)
                                {
                                    parametersValue += $"{executing.Parameters[i].ParameterName}:{executing.Parameters[i].Value}" + ";\n";
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(parametersValue))
                            {
                                //Console.WriteLine(parametersValue);
                                log.LogDebug
                             (
                                 "\n=================================================================================\n\n"
                                                             + executing.CommandText + "\n\n"
                                                             + "\n" + parametersValue +
                                 "\n=================================================================================\n\n"
                             );
                            }
                            else
                            {
                                log.LogDebug
                                (
                                    "\n=================================================================================\n\n"
                                                                    + executing.CommandText +
                                    "\n\n=================================================================================\n"
                                );
                            }
                            //Console.WriteLine("=================================================================================\n");
                            //Console.ResetColor();
                        }
                    });
                if (config.SlaveConnections.Count > 0)//判断是否存在从库
                {
                    builder.UseSlave(config.SlaveConnections.Select(x => x.ConnectionString).ToArray());
                }
                var res = builder.Build<T>();

                #region //使用FreeSql AOP做对应的业务拓展，有需要自行实现

                //res.GlobalFilter.Apply<IDeleted>(SysConsts.IsDeletedDataFilter, x => !x.IsDeleted);
                //res.GlobalFilter.Apply<IEnabled>(SysConsts.IsEnabledDataFilter, x => x.Enabled == true);
                //res.Aop.ConfigEntity += new EventHandler<ConfigEntityEventArgs>((_, e) =>
                //{
                //    var attrs = e.EntityType.GetCustomAttributes(typeof(IndexAttribute), false);
                //    foreach (var attr in attrs)
                //    {
                //        var temp = attr as IndexAttribute;
                //        e.ModifyIndexResult.Add(new FreeSql.DataAnnotations.IndexAttribute(temp.Name, temp.Fields, temp.IsUnique));
                //    }
                //});

                #endregion //使用FreeSql AOP做对应的业务拓展，有需要自行实现

                return res;
            });

            service.TryAddScoped<IUnitOfWork<T>, UnitOfWork<T>>();
        }




        /// <summary>
        /// 普通仓储模式
        /// </summary>
        /// <param name="service"></param>
        /// <param name="key"></param>
        public static void AddFreeSqlRepository<T>(this IServiceCollection service)
        {

            //var thisFontColor = (int)Console.ForegroundColor;
            //var newFontColor = (ConsoleColor)((thisFontColor + 1) > 15 ? 0 : (thisFontColor + 1));

            //var config = configuration.GetSection("SqlConfig").Get<FreeSqlCollectionConfig>()
            // ?.FreeSqlCollections.FirstOrDefault(x => x.Key == typeof(T).Name);

            //if (config == null)
            //    throw new InvalidOperationException($"未找到 {typeof(T).Name} 的 SqlConfig 配置");
            //注入FreeSql
            service.AddScoped(serviceProvider=>
            {
                var options = serviceProvider.GetRequiredService<IOptions<FreeSqlCollectionConfig>>();
                var log = serviceProvider.GetRequiredService<ILogger<IFreeSql>>();
                var config = options.Value.FreeSqlCollections.FirstOrDefault(x => x.Key == typeof(T).Name);
                if (config == null)
                    throw new InvalidOperationException($"未找到 {typeof(T).Name} 的 SqlConfig 配置");
                var freeSql =serviceProvider.GetRequiredService<IOptions<FreeSqlCollectionConfig>>().Value;
                var freeBuilder = new FreeSqlBuilder()
                    .UseAutoSyncStructure(config.IsSyncStructure)
                    .UseConnectionString(config.DataType, config.MasterConnetion)
                    .UseLazyLoading(config.IsLazyLoading)
                    .UseMonitorCommand(executing =>
                    {
                        //Console.ForegroundColor = newFontColor;
                        //Console.WriteLine("=================================================================================\n");
                        //Console.WriteLine(aop.CommandText + "\n");
                        executing.CommandTimeout = config.CommandTimeout;

                        if (config.DebugShowSql)
                        {

                            string parametersValue = "";
                            for (int i = 0; i < executing.Parameters.Count; i++)
                            {
                                parametersValue += $"{executing.Parameters[i].ParameterName}:{executing.Parameters[i].Value}" + ";\n";
                            }
                            if (!string.IsNullOrWhiteSpace(parametersValue))
                            {
                                //Console.WriteLine(parametersValue);

                                log.LogInformation
                                (
                                    "\n=================================================================================\n\n"
                                                                + executing.CommandText + "\n\n"
                                                                + parametersValue +
                                    "\n=================================================================================\n\n"
                                );
                            }

                            log.LogInformation
                            (
                                "\n=================================================================================\n\n"
                                                                                + executing.CommandText +
                                "\n\n=================================================================================\n"
                            );

                            //Console.WriteLine("=================================================================================\n");
                            //Console.ForegroundColor = (ConsoleColor)thisFontColor;
                        }
                    });
                if (config.SlaveConnections?.Count > 0)//判断是否存在从库
                {
                    freeBuilder.UseSlave(config.SlaveConnections.Select(x => x.ConnectionString).ToArray());
                }
                var freesql = freeBuilder.Build();
                return freesql;


            });


            service.TryAddScoped(sp =>
            {
                var options = sp.GetRequiredService<IOptions<FreeSqlCollectionConfig>>();
                var config = options.Value.FreeSqlCollections.FirstOrDefault(x => x.Key == typeof(T).Name);
                if (config.IsUseUnitOfWork)
                {
                    return sp.GetRequiredService<IFreeSql>().CreateUnitOfWork();
                }
                return null;

            });
        }



        //public static void AddFreeSql(this IServiceCollection service, string dbName)
        //{

        //    var thisFontColor = (int)Console.ForegroundColor;
        //    var newFontColor = (ConsoleColor)((thisFontColor + 1) > 15 ? 0 : (thisFontColor + 1));
        //    service.TryAddScoped<IFreeSqlUnitOfWorkManager, FreeSqlUnitOfWorkManager>();
        //    if (service == null) throw new ArgumentNullException(nameof(service));


        //    //注入FreeSql
        //    service.AddScoped(f =>
        //    {
        //        var log = f.GetRequiredService<ILogger<IFreeSql>>();
        //        var current = f.GetRequiredService<IOptions<FreeSqlCollectionConfig>>().Value.FreeSqlCollections.FirstOrDefault(x => x.Key == dbName);

        //        var freeBuilder = new FreeSqlBuilder()
        //            .UseAutoSyncStructure(current.IsSyncStructure)
        //            .UseConnectionString(current.DataType, current.MasterConnetion)
        //            .UseLazyLoading(current.IsLazyLoading)
        //            .UseMonitorCommand(aop =>
        //            {
        //                if (current.DebugShowSql)
        //                {

        //                    //Console.ForegroundColor = newFontColor;
        //                    //Console.WriteLine("=================================================================================\n");
        //                    //Console.WriteLine(aop.CommandText + "\n");

        //                    string parametersValue = "";
        //                    for (int i = 0; i < aop.Parameters.Count; i++)
        //                    {
        //                        parametersValue += $"{aop.Parameters[i].ParameterName}:{aop.Parameters[i].Value}" + ";\n";
        //                    }
        //                    if (!string.IsNullOrWhiteSpace(parametersValue))
        //                    {
        //                        //Console.WriteLine(parametersValue);

        //                        log.LogInformation
        //                        (
        //                            "\n=================================================================================\n\n"
        //                                                        + aop.CommandText + "\n\n"
        //                                                        + parametersValue +
        //                            "\n=================================================================================\n\n"
        //                        );
        //                    }

        //                    log.LogInformation
        //                    (
        //                        "\n=================================================================================\n\n"
        //                                                                        + aop.CommandText +
        //                        "\n\n=================================================================================\n"
        //                    );

        //                    //Console.WriteLine("=================================================================================\n");
        //                    //Console.ForegroundColor = (ConsoleColor)thisFontColor;
        //                }
        //            });
        //        if (current.SlaveConnections?.Count > 0)//判断是否存在从库
        //        {
        //            freeBuilder.UseSlave(current.SlaveConnections.Select(x => x.ConnectionString).ToArray());
        //        }
        //        var freesql = freeBuilder.Build();
        //        //我这里禁用了导航属性联级插入的功能
        //        freesql.SetDbContextOptions(opt => opt.EnableCascadeSave = false);
        //        return freesql;
        //    });

        //    //注入Uow
        //    service.AddScoped(f => f.GetRequiredService<IFreeSql>().CreateUnitOfWork());
        //}

    }
}