using Contracts;
using MassTransit;
using SearchService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

//add autoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//adding Mass Transit to communicate with RabbitMQ
// we don't need authentication to message bus at this point
builder.Services.AddMassTransit(x=>
{
    // define the consumer namespace. any other consumer we create under the same namespace will be registered automatically
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    // I don't like default formatting of our names for consumers
    // because if we create another consumer with the same name in a different service, then each time I need to add extra names before the class name to be different from other services class name
    // to use a prefix that will be added in front of the queue name
    // we add Search that represents the name of the service that this queue represents.
    // false means, do we want to include namespace with the formatted name
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("Search",false));
    // configuration for mass transit
    x.UsingRabbitMq((context,cfg)=>
    {
        //adding retry logic - remember Kebab case resulted in dashed between the endpoint
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            // if failed, retry 5 times max in 5 second intervals
            e.UseMessageRetry(r => r.Interval(5,5));
            // this applies this endpoint formatting to AuctionCreatedConsumer only
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();
app.MapControllers();

try
{
    await DbInitializer.InitDb(app);
}
catch(Exception ex)
{
    Console.WriteLine(ex);
}
app.Run();
