using Common.DistribuitedCache;
using DataAccess;
using DataWriterService.Services;
using Events.Messages;
using MessageBus.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.ConfigureRabbitClient(builder.Configuration);
builder.Services.AddRabbitMessageConsumer<RegisterUser, RegisterUserMessageConsumer>();
builder.Services.AddRabbitMessageConsumer<UpdateUserScore, UpdateUserMessageConsumer>();

builder.Services.AddNoSqlRepository(builder.Configuration, withSession: false);

builder.Services.AddRedisCache(builder.Configuration);

var app = builder.Build();
app.UseHealthChecks("/status/health");
app.Run();
