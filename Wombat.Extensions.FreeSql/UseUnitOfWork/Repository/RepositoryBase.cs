using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FreeSql;
using Microsoft.Extensions.DependencyInjection;

namespace Wombat.Extensions.FreeSql
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="Context"></typeparam>
    public abstract class RepositoryBase<TEntity, Context> : UowCoreRepositoryBase<TEntity, Context>
        where TEntity : class
    {
        public RepositoryBase(IServiceProvider service) : base(service)
        {
            base.UnitOfWork = service.GetRequiredService<IUnitOfWork<Context>>();
        }
    }






    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="Context"></typeparam>
    public abstract class UowCoreRepositoryBase<TEntity, Context> : BaseRepository<TEntity>, IRepositoryBase<TEntity, Context>
       where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        public UowCoreRepositoryBase(IServiceProvider service) : base(service.GetRequiredService<IFreeSql<Context>>())
        {
            base.UnitOfWork = service.GetRequiredService<IUnitOfWork<Context>>();
        }



        #region Uow CUD操作拓展

        /// <summary>
        /// 插入异步 单体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task<int> UowInsertAsync<T>(T entity) where T : class
        {
            return Orm.Insert(entity).WithTransaction(UnitOfWork.GetOrBeginTransaction()).ExecuteAffrowsAsync();
        }
        /// <summary>
        /// 插入同步 单体 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int UowInsert<T>(T entity) where T : class
        {
            return Orm.Insert(entity).WithTransaction(UnitOfWork.GetOrBeginTransaction()).ExecuteAffrows();
        }
        /// <summary>
        /// 插入 异步 集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public Task<int> UowInsertAsync<T>(IEnumerable<T> entitys) where T : class
        {

            return Orm.Insert(entitys).WithTransaction(UnitOfWork.GetOrBeginTransaction()).ExecuteAffrowsAsync();
        }
        /// <summary>
        /// 插入 同步 集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public int UowInsert<T>(IEnumerable<T> entitys) where T : class
        {
            return Orm.Insert(entitys).WithTransaction(UnitOfWork.GetOrBeginTransaction()).ExecuteAffrows();
        }
        /// <summary>
        /// 删除单体异步
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task<int> UowDeleteAsync<T>(T entity) where T : class
        {
            return Orm.Delete<T>(entity).WithTransaction(UnitOfWork.GetOrBeginTransaction()).ExecuteAffrowsAsync();
        }
        /// <summary>
        /// 删除 同步 单体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int UowDelete<T>(T entity) where T : class
        {
            return Orm.Delete<T>(entity).WithTransaction(UnitOfWork.GetOrBeginTransaction()).ExecuteAffrows();
        }
        /// <summary>
        /// 删除 异步 集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public Task<int> UowDeleteAsync<T>(IEnumerable<T> entitys) where T : class
        {
            return Orm.Delete<T>(entitys).WithTransaction(UnitOfWork.GetOrBeginTransaction()).ExecuteAffrowsAsync();
        }
        /// <summary>
        /// 删除同步 集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public int UowDelete<T>(IEnumerable<T> entitys) where T : class
        {
            return Orm.Delete<T>(entitys).WithTransaction(UnitOfWork.GetOrBeginTransaction()).ExecuteAffrows();
        }

        /// <summary>
        /// 更新 集合 异步
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public Task<int> UowUpdateAsync<T>(IEnumerable<T> entitys) where T : class
        {
            return Orm.Update<T>(new[] { entitys }).WithTransaction(UnitOfWork.GetOrBeginTransaction()).ExecuteAffrowsAsync();
        }
        /// <summary>
        /// 更新 异步 单体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public Task<int> UowUpdateAsync<T>(T entity) where T : class
        {
            return Orm.Update<T>(entity).WithTransaction(UnitOfWork.GetOrBeginTransaction()).ExecuteAffrowsAsync();
        }
        /// <summary>
        /// 更新 同步 集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entitys"></param>
        /// <returns></returns>
        public int UowUpdate<T>(IEnumerable<T> entitys) where T : class
        {
            return Orm.Update<T>(new[] { entitys }).WithTransaction(UnitOfWork.GetOrBeginTransaction()).ExecuteAffrows();
        }
        /// <summary>
        /// 更新 同步 单体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int UowUpdate<T>(T entity) where T : class
        {
            return Orm.Update<T>(entity).WithTransaction(UnitOfWork.GetOrBeginTransaction()).ExecuteAffrows();
        }

        #endregion


    }
}
