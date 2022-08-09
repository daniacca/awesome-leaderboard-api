using AwesomeLeaderboard.Services;
using Common.DistribuitedCache;
using DataAccess;
using Events.Messages;
using MessageBus.RabbitMq;
using MessageBus.RabbitMq.AbsClasses;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

var builder = WebApplication.CreateBuilder(args);

// Configure Web API endpoints
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure RabbitMQ services
builder.Services.ConfigureRabbitClient(builder.Configuration);
builder.Services.AddRabbitProducer<RegisterUser, RabbitProducerBase<RegisterUser>, RegisterUserMessageProducer>(ServiceLifetime.Scoped);
builder.Services.AddRabbitProducer<UpdateUserScore, RabbitProducerBase<UpdateUserScore>, UpdateUserMessageProducer>(ServiceLifetime.Scoped);

// Configure MongoDB services
builder.Services.AddNoSqlRepository(builder.Configuration);

// Configure Redis services
builder.Services.AddRedisCache(builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseHealthChecks("/status/health");

app.Run();