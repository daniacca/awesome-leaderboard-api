using Common.NoSql.Commands;
using MongoDB.Bson;
using MongoDB.Driver;
using NaTourWine.Core.Repositories.NoSql.Querying;
using NoSql.MongoDb.Abstraction.AbstractClasses;
using NoSql.MongoDb.Abstraction.Interfaces;
using NoSql.MongoDb.Querying;
using NoSql.MongoDb.Types;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace NoSql.MongoDb.Repository
{
    public class NoSqlRepository<TEntity> : INoSqlRepository<TEntity> where TEntity : class, INoSqlCollection, new()
    {
        protected INoSqlSessionProvider Session { get; }
        protected INoSqlDBContext<TEntity> Context { get; }

        protected NoSqlRepository(INoSqlDBContext<TEntity> dBContext, INoSqlSessionProvider session)
        {
            Context = dBContext;
            Session = session;
        }

        protected bool IsEntityCapped => Context.IsCapped;

        protected virtual TCommand CommandBuilder<TCommand>() where TCommand : class
        {
            return (TCommand)Activator.CreateInstance(typeof(TCommand), new object[] { Context, Session });
        }

        public virtual INoSqlDBContext<TEntity> GetDBContext()
        {
            return Context;
        }

        public virtual bool Add(TEntity obj, IClientSessionHandle session = null)
        {
            var command = CommandBuilder<InsertCommand<TEntity>>();
            return command.Execute(obj);
        }

        public virtual async Task<bool> AddAsync(TEntity obj, IClientSessionHandle session = null)
        {
            var command = CommandBuilder<InsertCommand<TEntity>>();
            return await command.ExecuteAsync(obj);
        }

        public virtual bool Remove(TEntity obj, IClientSessionHandle session = null)
        {
            if (IsEntityCapped)
                throw new NotSupportedException("Capped collection not support this operation");

            var command = CommandBuilder<DeleteCommand<TEntity>>();
            return command.Execute(obj);
        }

        public virtual async Task<bool> RemoveAsync(TEntity obj, IClientSessionHandle session = null)
        {
            if (IsEntityCapped)
                throw new NotSupportedException("Capped collection not support this operation");

            var command = CommandBuilder<DeleteCommand<TEntity>>();
            return await command.ExecuteAsync(obj);
        }

        public virtual bool Update(TEntity obj, IClientSessionHandle session = null)
        {
            var command = CommandBuilder<UpdateCommand<TEntity>>();
            return command.Execute(obj);
        }

        public virtual async Task<bool> UpdateAsync(TEntity obj, IClientSessionHandle session = null)
        {
            var command = CommandBuilder<UpdateCommand<TEntity>>();
            return await command.ExecuteAsync(obj);
        }

        public virtual (IEnumerable<TEntity> data, int records, int pages) GetList(string where, string orderby = null, int? pageSize = null, int? pageNumber = null)
        {
            var command = CommandBuilder<GetListQuerying<INoSqlLinqDynamicFilter, TEntity>>();
            var param = new NoSqlLinqDynamicFilter { Where = where, OrderBy = orderby, PageNumber = pageNumber, PageSize = pageSize };
            return command.Execute(param);
        }

        public virtual async Task<(IEnumerable<TEntity> data, int records, int pages)> GetListAsync(string where, string orderby = null, int? pageSize = null, int? pageNumber = null)
        {
            var command = CommandBuilder<GetListQuerying<INoSqlLinqDynamicFilter, TEntity>>();
            var param = new NoSqlLinqDynamicFilter { Where = where, OrderBy = orderby, PageNumber = pageNumber, PageSize = pageSize };
            return await command.ExecuteAsync(param);
        }

        public virtual TEntity Get(string id)
        {
            var command = CommandBuilder<GetQuerying<TEntity>>();
            return command.Execute(id);
        }

        public virtual async Task<TEntity> GetAsync(string id)
        {
            var command = CommandBuilder<GetQuerying<TEntity>>();
            return await command.ExecuteAsync(id);
        }

        public virtual IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> predicate)
        {
            var command = CommandBuilder<GetListLinqQuerying<TEntity>>();
            return command.Execute(predicate);
        }

        public virtual async Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var command = CommandBuilder<GetListLinqQuerying<TEntity>>();
            return await command.ExecuteAsync(predicate);
        }

        public virtual bool Add(IEnumerable<TEntity> list, IClientSessionHandle session = null)
        {
            var command = CommandBuilder<BulkInsertCommand<TEntity>>();
            return command.Execute(list);
        }

        public virtual async Task<bool> AddAsync(IEnumerable<TEntity> list, IClientSessionHandle session = null)
        {
            var command = CommandBuilder<BulkInsertCommand<TEntity>>();
            return await command.ExecuteAsync(list);
        }

        public virtual bool Update(IEnumerable<TEntity> list, IClientSessionHandle session = null)
        {
            if (IsEntityCapped)
                throw new NotSupportedException("Capped collection not support this operation");

            var command = CommandBuilder<BulkUpdateCommand<TEntity>>();
            return command.Execute(list);
        }

        public virtual async Task<bool> UpdateAsync(IEnumerable<TEntity> list, IClientSessionHandle session = null)
        {
            if (IsEntityCapped)
                throw new NotSupportedException("Capped collection not support this operation");

            var command = CommandBuilder<BulkUpdateCommand<TEntity>>();
            return await command.ExecuteAsync(list);
        }

        public virtual bool Remove(IEnumerable<TEntity> list, IClientSessionHandle session = null)
        {
            if (IsEntityCapped)
                throw new NotSupportedException("Capped collection not support this operation");

            var command = CommandBuilder<BulkDeleteCommand<TEntity>>();
            return command.Execute(list);
        }

        public virtual async Task<bool> RemoveAsync(IEnumerable<TEntity> list, IClientSessionHandle session = null)
        {
            if (IsEntityCapped)
                throw new NotSupportedException("Capped collection not support this operation");

            var command = CommandBuilder<BulkDeleteCommand<TEntity>>();
            return await command.ExecuteAsync(list);
        }

        public virtual IEnumerable<TOutput> GetList<TOutput>(IEnumerable<BsonDocument> aggregatePipeline)
        {
            var command = CommandBuilder<PipelineQuerying<TEntity, TOutput>>();
            return command.Get(aggregatePipeline);
        }

        public virtual async Task<IEnumerable<TOutput>> GetListAsync<TOutput>(IEnumerable<BsonDocument> aggregatePipeline)
        {
            var command = CommandBuilder<PipelineQuerying<TEntity, TOutput>>();
            return await command.GetAsync(aggregatePipeline);
        }

        public virtual IEnumerable<TOut> GetList<TOut>(IEnumerable<AbsAggregationPipelineStage> stages) where TOut: class
        {
            var command = CommandBuilder<PipelineQueryingFluent<TEntity, TOut>>();
            return command.Get(stages);
        }

        public async virtual Task<IEnumerable<TOut>> GetListAsync<TOut>(IEnumerable<AbsAggregationPipelineStage> stages) where TOut : class
        {
            var command = CommandBuilder<PipelineQueryingFluent<TEntity, TOut>>();
            return await command.GetAsync(stages);
        }

        public virtual bool ValidateHash(string id, uint hash)
        {
            if (typeof(INoSqlSecurityCollection).IsAssignableFrom(typeof(TEntity)))
            {
                var entity = new TEntity { _id = id };
                if (entity is INoSqlSecurityCollection entitySecurity)
                    entitySecurity.Hash = hash;

                var command = CommandBuilder<GetQuerying<TEntity>>();
                return command.ValidateHash(entity);
            }

            return true;
        }

        public void CalculateHash(TEntity obj)
        {
            if (typeof(INoSqlSecurityCollection).IsAssignableFrom(typeof(TEntity)))
            {
                var command = CommandBuilder<GetQuerying<TEntity>>();
                command.CalculateHash(obj);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
