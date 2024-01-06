using AuctionService;
using AuctionService.Data;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
//adding autoMapper
// the argument passed in AddAutoMapper provides the location of the assembly that this application is running in and looks for any classes that drives from Profile 
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// adding mass transit for rabbitMQ communication
// we don't need authentication to message bus at this point
builder.Services.AddMassTransit(x=>
{
    // define the consumer namespace. any other consumer we create under the same namespace will be registered automatically
    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    x.AddConsumersFromNamespaceContaining<AuctionUpdatedFaultConsumer>();
    // modifying the default name formatting of the Consumers
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("Auction",false));
    // adding the outbox for Auction Db and if there is any message in outbox, every 10 seconds it will check and try to deliver it to service bus
    x.AddEntityFrameworkOutbox<AuctionDbContext>(o=>{
        o.QueryDelay = TimeSpan.FromSeconds(10);
        // we want to use postgres. 
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.UsingRabbitMq((context,cfg)=>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

// Seed data in our db
try
{
    DbInitializer.InitDb(app);
}
catch(Exception ex)
{
    Console.WriteLine(ex);
}


app.Run();

