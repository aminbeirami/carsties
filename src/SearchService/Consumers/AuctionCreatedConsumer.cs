using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private readonly IMapper _mapper;
    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine($"--> Consuming action created: {context.Message.Id}");
        var item = _mapper.Map<Item>(context.Message);

        // In some cases exceptions will happen. here is just to show how to handle these exceptions
        // for example in this case, a car with name Foo is added to Message queue, we should throw an exception when we get that
        //in this case we have an inconsistant data
        if(item.Model == "Foo") throw new ArgumentException("Cannot sell cars with the name Foo");
        await item.SaveAsync();
    }
}
