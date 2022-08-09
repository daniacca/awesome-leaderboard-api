namespace NoSql.MongoDb.Abstraction.Interfaces
{
    public interface INoSqlHashValidator<T>
    {
        bool ValidateHash(T param);
    }
}
