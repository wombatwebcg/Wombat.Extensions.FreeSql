using System;
using System.Collections.Generic;
using System.Text;
using FreeSql;
using Microsoft.Extensions.DependencyInjection;

namespace Wombat.Extensions.FreeSql
{
    public class UnitOfWork<Context> : UnitOfWork, IUnitOfWork<Context>
    {
        private IServiceProvider _serviceProvider;
        /// <summary>
        /// 追踪号
        /// </summary>
        public UnitOfWork(IServiceProvider service) : base(service.GetRequiredService<IFreeSql<Context>>())
        {
            _serviceProvider = service;
            Create<IFreeSqlUnitOfWorkManager>().Register(this);
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        private T Create<T>()
        {
            var result = _serviceProvider.GetService<T>();
            if (result == null)
            {
                return default(T);
            }
            else
            {
                return result;
            }
        }
    }
}
