using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using NoSql.MongoDb.ExtensionMethods;

namespace NoSql.MongoDb.NameConventions
{
    public class CamelCaseNameConvention : IMemberMapConvention, IConvention
    {
        public string Name => "CustomCamelCase";

        public void Apply(BsonMemberMap memberMap)
        {
            memberMap.SetElementName(memberMap.MemberName.ToCamelCase());
        }
    }
}
