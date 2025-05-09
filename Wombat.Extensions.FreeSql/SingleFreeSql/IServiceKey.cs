using FreeSql;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wombat.Extensions.FreeSql
{
    public interface IServiceKey
    {
        IRepositoryUnitOfWork UnitOfWork { get; }
    }
}
