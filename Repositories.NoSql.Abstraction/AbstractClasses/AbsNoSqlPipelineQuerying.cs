using NeoSmart.Hashing.XXHash.Core;
using NoSql.MongoDb.Abstraction.Interfaces;
using System.Text;

namespace Common.NoSql.AbstractClasses
{
    public abstract class AbsNoSqlPipelineQuerying
    {
        protected INoSqlSessionProvider Session { get; }

        public AbsNoSqlPipelineQuerying(INoSqlSessionProvider session)
        {
            Session = session;
        }

        protected void CalculateHash<T>(T obj)
        {
            if (obj is INoSqlSecurityCollection securityParam)
            {
                string messageToSign = $"{securityParam._id}{Session?.AccountId}{Session?.SecretHashKey}";
                securityParam.Hash = XXHash.XXH32(Encoding.ASCII.GetBytes(messageToSign));
            }
        }
    }
}
