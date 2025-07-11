﻿using System;
using System.Collections.Generic;
using FreeSql;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Common;

namespace Wombat.Extensions.FreeSql
{
    public class BaseRepository<T, TKey> : IRepositoryKey<T> where T : class
    {
        private IBaseRepository<T> _baseRep;
       private IServiceProvider _service;
        public BaseRepository(IServiceProvider service)
        {
            var fsql = service.GetRequiredService<IFreeSql>();
            _baseRep = fsql.GetRepository<T>();
            UnitOfWork = fsql?.CreateUnitOfWork();
        }

        public IUpdate<T> UpdateDiy => _baseRep.UpdateDiy;

        public RepositoryDataFilter DataFilter => _baseRep.DataFilter;

        public ISelect<T> Select => _baseRep.Select;

        public Type EntityType => _baseRep.EntityType;
        public IUnitOfWork UnitOfWork 
        { 
            get; 
            
            set;
        
        }

        public IFreeSql Orm => _baseRep.Orm;

        public DbContextOptions DbContextOptions { get => _baseRep.DbContextOptions; set => _baseRep.DbContextOptions = value; }

        public void AsTable(Func<string, string> rule) => _baseRep.AsTable(rule);

        public void AsTable(Func<Type, string, string> rule)
        {
            _baseRep.AsTable(rule);
        }

        public void AsType(Type entityType) => _baseRep.AsType(entityType);

        public void Attach(T entity) => _baseRep.Attach(entity);

        public void Attach(IEnumerable<T> entity) => _baseRep.Attach(entity);

        public IBaseRepository<T> AttachOnlyPrimary(T data) => _baseRep.AttachOnlyPrimary(data);

        public void BeginEdit(List<T> data) => _baseRep.BeginEdit(data);

        public Dictionary<string, object[]> CompareState(T newdata)
        {
            return _baseRep.CompareState(newdata);
        }

        public int Delete(Expression<Func<T, bool>> predicate) => _baseRep.Delete(predicate);

        public int Delete(T entity)
        {
            return _baseRep.Delete(entity);
        }

        public int Delete(IEnumerable<T> entitys)
        {
            return _baseRep.Delete(entitys);
        }

        public Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
        {
            return _baseRep.DeleteAsync(predicate);
        }

        public Task<int> DeleteAsync(T entity)
        {
            return _baseRep.DeleteAsync(entity);
        }

        public Task<int> DeleteAsync(IEnumerable<T> entitys)
        {
            return _baseRep.DeleteAsync(entitys);
        }

        public Task<int> DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            return _baseRep.DeleteAsync(entity, cancellationToken);
        }

        public Task<int> DeleteAsync(IEnumerable<T> entitys, CancellationToken cancellationToken = default)
        {
            return _baseRep.DeleteAsync(entitys, cancellationToken);
        }

        public Task<int> DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return _baseRep.DeleteAsync(predicate, cancellationToken);
        }

        public List<object> DeleteCascadeByDatabase(Expression<Func<T, bool>> predicate)
        {
            return _baseRep.DeleteCascadeByDatabase(predicate);
        }

        public Task<List<object>> DeleteCascadeByDatabaseAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return _baseRep.DeleteCascadeByDatabaseAsync(predicate, cancellationToken);
        }

        public void Dispose()
        {
            _baseRep.Dispose();
        }

        public int EndEdit(List<T> data = null)
        {
            throw new NotImplementedException();
        }

        public void FlushState()
        {
            _baseRep.FlushState();
        }

        public T Insert(T entity)
        {
            return _baseRep.Insert(entity);
        }

        public List<T> Insert(IEnumerable<T> entitys)
        {
            return _baseRep.Insert(entitys);
        }

        public Task<T> InsertAsync(T entity)
        {
            return _baseRep.InsertAsync(entity);
        }

        public Task<List<T>> InsertAsync(IEnumerable<T> entitys)
        {
            return _baseRep.InsertAsync(entitys);
        }

        public Task<T> InsertAsync(T entity, CancellationToken cancellationToken = default)
        {
            return _baseRep.InsertAsync(entity, cancellationToken);
        }

        public Task<List<T>> InsertAsync(IEnumerable<T> entitys, CancellationToken cancellationToken = default)
        {
            return _baseRep.InsertAsync(entitys, cancellationToken);
        }

        public T InsertOrUpdate(T entity)
        {
            return _baseRep.InsertOrUpdate(entity);
        }

        public Task<T> InsertOrUpdateAsync(T entity)
        {
            return _baseRep.InsertOrUpdateAsync(entity);
        }

        public Task<T> InsertOrUpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            return _baseRep.InsertOrUpdateAsync(entity, cancellationToken);
        }

        public void SaveMany(T entity, string propertyName)
        {
            _baseRep.SaveMany(entity, propertyName);
        }

        public Task SaveManyAsync(T entity, string propertyName)
        {
            return _baseRep.SaveManyAsync(entity, propertyName);
        }

        public Task SaveManyAsync(T entity, string propertyName, CancellationToken cancellationToken = default)
        {
            return _baseRep.SaveManyAsync(entity, propertyName, cancellationToken);
        }

        public int Update(T entity)
        {
            return _baseRep.Update(entity);
        }

        public int Update(IEnumerable<T> entitys)
        {
            return _baseRep.Update(entitys);
        }

        public Task<int> UpdateAsync(T entity)
        {
            return _baseRep.UpdateAsync(entity);
        }

        public Task<int> UpdateAsync(IEnumerable<T> entitys)
        {
            return _baseRep.UpdateAsync(entitys);
        }

        public Task<int> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            return _baseRep.UpdateAsync(entity, cancellationToken);
        }

        public Task<int> UpdateAsync(IEnumerable<T> entitys, CancellationToken cancellationToken = default)
        {
            return _baseRep.UpdateAsync(entitys, cancellationToken);
        }

        public ISelect<T> Where(Expression<Func<T, bool>> exp)
        {
            return _baseRep.Where(exp);
        }

        public ISelect<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
        {
            return _baseRep.WhereIf(condition, exp);
        }
    }
}