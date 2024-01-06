using Contracts;
using MassTransit;

namespace AuctionService;

public class AuctionUpdatedFaultConsumer : IConsumer<Fault<AuctionUpdated>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionUpdated>> context)
    {
        Console.WriteLine("--> Consuming faulty Update");
        var exception = context.Message.Exceptions.First();
        // we check the type of exception and it should be what we threw in Search Service
        if(exception.ExceptionType == "System.ArgumentException")
        {
            // retry
            await context.Publish(context.Message.Message);
        }else{
            Console.WriteLine("Not an Argument Exception");
        }
    }
}
