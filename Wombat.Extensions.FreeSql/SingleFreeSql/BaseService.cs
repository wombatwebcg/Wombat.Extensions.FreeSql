using System;
using System.Collections.Generic;
using System.Text;
using FreeSql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Wombat.Extensions.FreeSql
{
    public class BaseService : IServiceKey
    {
        //public ILogger Logger { get; }

        public IRepositoryUnitOfWork UnitOfWork { get; }

        public BaseService(IServiceProvider service)
        {
  
            UnitOfWork = service.GetRequiredService<IRepositoryUnitOfWork>();
        }
    }
}
