using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FreeSql;

namespace Wombat.Extensions.FreeSql
{
    public interface IFreeSqlUnitOfWorkManager : IDisposable
    {
        /// <summary>
        /// 提交
        /// </summary>
        void Commit();
        /// <summary>
        /// 提交
        /// </summary>
        Task CommitAsync();
        /// <summary>
        /// 注册工作单元
        /// </summary>
        /// <param name="unitOfWork">工作单元</param>
        void Register(IUnitOfWork unitOfWork);
    }
}
