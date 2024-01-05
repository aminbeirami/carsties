using Contracts;
using MassTransit;

namespace AuctionService;
//This is just an example and used for learning purposes
public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
    // context contains the faulty message
    public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        Console.WriteLine("--> Consuming faulty creation");
        // we get the exception first
        var exception = context.Message.Exceptions.First();
        // we check the type of exception and it should be what we threw in Search Service
        if(exception.ExceptionType == "System.ArgumentException")
        {
            // in this case rename the model. you can do whatever you want with this
            context.Message.Message.Model = "FoorBar";
            // republish it to rabbitMq
            await context.Publish(context.Message.Message);
        }else{
            Console.WriteLine("Not an Argument Exception");
        }
    }
}
