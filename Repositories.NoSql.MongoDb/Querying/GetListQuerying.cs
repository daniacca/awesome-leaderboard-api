using Common.NoSql.AbstractClasses;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using NoSql.MongoDb.Abstraction.Interfaces;

namespace NaTourWine.Core.Repositories.NoSql.Querying
{
    class GetListQuerying<TIn, TOut> : AbsNoSqlQuerying<TIn, (IEnumerable<TOut> data, int records, int pages), TOut>, INoSqlActionAsync<(IEnumerable<TOut> data, int records, int pages), TIn>
        where TIn : class, INoSqlLinqDynamicFilter
        where TOut : class, INoSqlCollection, new()
    {
        public GetListQuerying(INoSqlDBContext<TOut> dBContext, INoSqlSessionProvider session) : base(dBContext, session)
        { }

        private (string predicate, object[] args) PreProcessWhereClause(string where)
        {
            var resultArgs = new List<object>();
            var resultWhere = where;
            var counter = 0;

            var cyclePattern = $"(?<=Contains\\(\\')(.*?)(?=\\s*\\'\\))";
            var cycleMatch = Regex.Match(resultWhere, cyclePattern, RegexOptions.IgnoreCase);
            while (cycleMatch.Success)
            {
                resultArgs.Add(cycleMatch.Value);
                resultWhere = resultWhere.Replace($"'{cycleMatch.Value}'", $"@{counter++}");
                cycleMatch = Regex.Match(resultWhere, cyclePattern, RegexOptions.IgnoreCase);
            }

            return (resultWhere, resultArgs.ToArray());
        }

        public override (IEnumerable<TOut> data, int records, int pages) Execute(TIn param, IClientSessionHandle session = null)
        {
            var (predicate, args) = PreProcessWhereClause(param.Where);
            var data = Collection.AsQueryable().Where(predicate, args);
            if (param.OrderBy is not null)
                data = data.OrderBy(param.OrderBy);
            
            var totalRecords = data.Count();
            var totalPages = 1;
            if (param.PageNumber is not null && param.PageSize is not null)
            {
                data = data
                    .Skip(param.PageSize.Value * (param.PageNumber.Value > 0 ? param.PageNumber.Value - 1 : param.PageNumber.Value))
                    .Take(param.PageSize.Value);
                
                totalPages = (totalRecords / param.PageSize.Value) + 1;
            }

            var result = data.ToList();
            result.ForEach(d => CalculateHash(d));
            return (data: result, records: totalRecords, pages: totalPages);
        }

        public override async Task<(IEnumerable<TOut> data, int records, int pages)> ExecuteAsync(TIn param, IClientSessionHandle session = null)
        {
            var totalPages = 1;
            var totalRecords = 0;
            
            var query = await Task.Run(() =>
            {
                var (predicate, args) = PreProcessWhereClause(param.Where);
                var res = Collection.AsQueryable().Where(predicate, args) as IMongoQueryable<TOut>;
                if (param.OrderBy is not null)
                    res = res.OrderBy(param.OrderBy) as IMongoQueryable<TOut>;
                
                totalRecords = res.Count();
                if (param.PageNumber is not null && param.PageSize is not null)
                {
                    res = res
                        .Skip(param.PageSize.Value * (param.PageNumber.Value > 0 ? param.PageNumber.Value - 1 : param.PageNumber.Value))
                        .Take(param.PageSize.Value);

                    totalPages = (totalRecords / param.PageSize.Value) + 1;
                }

                return res;
            });

            var data = await query.ToListAsync();
            data.ForEach(d => CalculateHash(d));
            return (data, records: totalRecords, pages: totalPages);
        }
    }

    class GetListLinqQuerying<TOut> : AbsNoSqlQuerying<Expression<Func<TOut, bool>>, IEnumerable<TOut>, TOut>, INoSqlActionAsync<IEnumerable<TOut>, Expression<Func<TOut, bool>>> where TOut : class, INoSqlCollection, new()
    {
        public GetListLinqQuerying(INoSqlDBContext<TOut> dBContext, INoSqlSessionProvider session) : base(dBContext, session)
        { }

        public override IEnumerable<TOut> Execute(Expression<Func<TOut, bool>> param, IClientSessionHandle session = null)
        {
            var data = Collection.AsQueryable().Where(param).ToList();
            data.ForEach(d => CalculateHash(d));
            return data;
        }

        public override async Task<IEnumerable<TOut>> ExecuteAsync(Expression<Func<TOut, bool>> param, IClientSessionHandle session = null)
        {
            var query = await Task.Run(() => Collection.AsQueryable().Where(param).ToList());
            query.ForEach(d => CalculateHash(d));
            return query;
        }
    }
}
