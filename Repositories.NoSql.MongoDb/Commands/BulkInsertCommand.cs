using Common.NoSql.AbstractClasses;
using MongoDB.Driver;
using NoSql.MongoDb.Abstraction.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.NoSql.Commands
{
    public class BulkInsertCommand<TIn> : AbsNoSqlCommand<IEnumerable<TIn>, bool, TIn> where TIn : class, INoSqlCollection, new()
    {
        private readonly InsertManyOptions insertOptions = new() 
        { 
            IsOrdered = false, 
            BypassDocumentValidation = true 
        };

        public BulkInsertCommand(INoSqlDBContext<TIn> dBContext, INoSqlSessionProvider session) : base(dBContext, session)
        { }

        public override bool Execute(IEnumerable<TIn> param, IClientSessionHandle session = null)
        {
            try
            {
                if (session is null)
                    Collection.InsertMany(param, insertOptions);
                else
                    Collection.InsertMany(session, param, insertOptions);

                param.ToList().ForEach(p => CalculateHash(p));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public override async Task<bool> ExecuteAsync(IEnumerable<TIn> param, IClientSessionHandle session = null)
        {
            try
            {
                if(session is null)
                    await Collection.InsertManyAsync(param, insertOptions);
                else
                    await Collection.InsertManyAsync(session, param, insertOptions);

                var tasks = param.ToList().Select(p => Task.Run(() => CalculateHash(p)));
                await Task.WhenAll(tasks);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
