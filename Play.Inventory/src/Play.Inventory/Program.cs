using MassTransit;
using Microsoft.Extensions.Options;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entites;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

Random jitterer = new();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(options => 
{
    options.SuppressAsyncSuffixInActionNames = false;
});

builder.Services.AddMongo()
    .AddMongoRepository<InventoryItem>("inventoryItems")
    .AddMongoRepository<CatalogItem>("catalogItems")
    .AddMassTransitWithRabbitMQ();

// Add service client
AddCatalogClient(builder, jitterer);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();



// service client action
static void AddCatalogClient(WebApplicationBuilder builder, Random jitterer)
{
    builder.Services.AddHttpClient<CatalogClient>(client =>
    {
        client.BaseAddress = new Uri("http://localhost:5229");
    })
    .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(
        5,
        retryAttempt => TimeSpan.FromMilliseconds(jitterer.Next(0, 1000))
    ))
    .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
        3,
        TimeSpan.FromSeconds(15)
    ))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10)));
}