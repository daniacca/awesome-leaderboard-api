using NoSql.MongoDb.Abstraction.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace NoSql.MongoDb.Types
{
    public class NoSqlSessionProvider : INoSqlSessionProvider
    {
        public string AccountId { get; } = "";

        public string SecretHashKey { get; } = "Sup35Sec3e_T";

        public NoSqlSessionProvider(IHttpContextAccessor httpContextAccessor, IOptions<MongoConnection> settings)
        {
            try
            {
                var claims = httpContextAccessor?.HttpContext?.User?.Claims;
                AccountId = claims?.First(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
                if (settings.Value.SecuritySecretKey is not null)
                    SecretHashKey = settings.Value.SecuritySecretKey;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
