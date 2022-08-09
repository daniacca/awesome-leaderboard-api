namespace NoSql.MongoDb.Types
{
    public class MongoConnection
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string SecuritySecretKey { get; set; }
    }
}
