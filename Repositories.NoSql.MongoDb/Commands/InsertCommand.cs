using Common.NoSql.AbstractClasses;
using MongoDB.Driver;
using System.Threading.Tasks;
using System;
using NoSql.MongoDb.Abstraction.Interfaces;

namespace Common.NoSql.Commands
{
    public class InsertCommand<TIn> : AbsNoSqlCommand<TIn, bool, TIn> where TIn : class, INoSqlCollection, new()
    {
        private readonly InsertOneOptions insertOptions = new()
        {
            BypassDocumentValidation = true
        };

        public InsertCommand(INoSqlDBContext<TIn> dBContext, INoSqlSessionProvider session) : base(dBContext, session)
        { }

        public override bool Execute(TIn param, IClientSessionHandle session = null)
        {
            try
            {
                if(session is null)
                    Collection.InsertOne(param, insertOptions);
                else
                    Collection.InsertOne(session, param, insertOptions);

                CalculateHash(param);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public override async Task<bool> ExecuteAsync(TIn param, IClientSessionHandle session = null)
        {
            try
            {
                if(session is null)
                    await Collection.InsertOneAsync(param, insertOptions);
                else
                    await Collection.InsertOneAsync(session, param, insertOptions);

                CalculateHash(param);
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
