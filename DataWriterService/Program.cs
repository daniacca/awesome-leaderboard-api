using Common.DistribuitedCache;
using DataAccess;
using DataWriterService.Services;
using Events.Messages;
using MessageBus.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

// General services
builder.Services.AddHealthChecks();

// Configuring Rabbit MQ services
builder.Services.ConfigureRabbitClient(builder.Configuration);
builder.Services.AddRabbitMessageConsumer<RegisterUser, RegisterUserMessageConsumer>();
builder.Services.AddRabbitMessageConsumer<UpdateUserScore, UpdateUserMessageConsumer>();

// Configuring MongoDB services
builder.Services.AddNoSqlRepository(builder.Configuration, withSession: false);

// Configure Redis services
builder.Services.AddRedisCache(builder.Configuration);

var app = builder.Build();
app.UseHealthChecks("/status/health");
app.Run();
