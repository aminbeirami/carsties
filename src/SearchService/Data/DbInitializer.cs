using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;

namespace SearchService;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        // initialize the MongoDb and name it SearchDb. and get the settings from app settings.
        await DB.InitAsync("SearchDb",MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        //Create bunch of indexes on certain properties of Item, to be able to search on.
        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();
        //Check if the db is empty
        var count = await DB.CountAsync<Item>();
        if(count == 0)
        {
            Console.WriteLine("No data. will attempt to seed");
            // Read data from json file udner same assmebly
            var itemData = await File.ReadAllTextAsync("Data/auctions.json");
            // make serialization case insensitive
            var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};
            var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);
            await DB.SaveAsync(items);
        }
    }
}
