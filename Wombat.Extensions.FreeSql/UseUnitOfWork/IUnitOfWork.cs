using System;
using System.Collections.Generic;
using System.Text;
using FreeSql;

namespace Wombat.Extensions.FreeSql
{
    public interface IUnitOfWork<Context> : IUnitOfWork
    {
    }
}
