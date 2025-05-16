using FreeSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wombat.Extensions.FreeSql
{
    /// <summary>
    /// 为了和FreeSql的仓储接口区分新增了这个接口
    /// 用来发现仓储的接口 并注入
    /// 仅此而已
    /// </summary>
    public interface IRepositoryKey: IBaseRepository
    {
    }

    /// <summary>
    /// 为了和FreeSql的仓储接口区分新增了这个接口
    /// 用来发现仓储的接口 并注入
    /// 仅此而已
    /// </summary>
    public interface IRepositoryKey<T> : IBaseRepository<T> where T : class
    {
    }
}
