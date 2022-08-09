using MongoDB.Bson;
using MongoDB.Driver;
using NoSql.MongoDb.Abstraction.AbstractClasses;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NoSql.MongoDb.Abstraction.Interfaces
{
    public interface INoSqlRepository<TEntity>: IDisposable where TEntity : class, INoSqlCollection, new()
    {
        #region IMongoDbContext - Helper function
        INoSqlDBContext<TEntity> GetDBContext();
        bool ValidateHash(string id, uint hash);
        void CalculateHash(TEntity obj);
        #endregion

        #region Single Command operation - Add / Update / Remove
        bool Add(TEntity obj, IClientSessionHandle session = null);
        Task<bool> AddAsync(TEntity obj, IClientSessionHandle session = null);
        bool Update(TEntity obj, IClientSessionHandle session = null);
        Task<bool> UpdateAsync(TEntity obj, IClientSessionHandle session = null);
        bool Remove(TEntity obj, IClientSessionHandle session = null);
        Task<bool> RemoveAsync(TEntity obj, IClientSessionHandle session = null);
        #endregion

        #region Bulk Command operation - Add / Update / Remove
        bool Add(IEnumerable<TEntity> list, IClientSessionHandle session = null);
        Task<bool> AddAsync(IEnumerable<TEntity> list, IClientSessionHandle session = null);
        bool Update(IEnumerable<TEntity> list, IClientSessionHandle session = null);
        Task<bool> UpdateAsync(IEnumerable<TEntity> list, IClientSessionHandle session = null);
        bool Remove(IEnumerable<TEntity> list, IClientSessionHandle session = null);
        Task<bool> RemoveAsync(IEnumerable<TEntity> list, IClientSessionHandle session = null);
        #endregion

        #region Querying Operation - Get-Single / Get-List / Get-Paging
        TEntity Get(string id);
        Task<TEntity> GetAsync(string id);
        IEnumerable<TEntity> GetList(Expression<Func<TEntity, bool>> where);
        Task<IEnumerable<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> where);
        (IEnumerable<TEntity> data, int records, int pages) GetList(string where, string orderby = null, int? pageSize = null, int? pageNumber = null);
        Task<(IEnumerable<TEntity> data, int records, int pages)> GetListAsync(string where, string orderby = null, int? pageSize = null, int? pageNumber = null);
        #endregion

        #region Aggregation Pipeline Operation
        Task<IEnumerable<TOutput>> GetListAsync<TOutput>(IEnumerable<BsonDocument> aggregatePipeline);
        IEnumerable<TOutput> GetList<TOutput>(IEnumerable<BsonDocument> aggregatePipeline);
        Task<IEnumerable<TOut>> GetListAsync<TOut>(IEnumerable<AbsAggregationPipelineStage> stages) where TOut : class;
        IEnumerable<TOut> GetList<TOut>(IEnumerable<AbsAggregationPipelineStage> stages) where TOut : class;
        #endregion
    }
}
