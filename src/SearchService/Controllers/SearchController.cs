using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using ZstdSharp.Unsafe;

namespace SearchService;
[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    //The reason we are not using IActionResult is that in that case we cannot pass type parameter like <Item> to it
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems([FromQuery]SearchParams searchParams)
    {
        // Read the documentation of mongodb-entities. we are using it here
        // in the filtering, we are adding order by. we need to define Item twice
        var query = DB.PagedSearch<Item, Item>();
        query.Sort(x => x.Ascending(a => a.Make));
        if(!string.IsNullOrEmpty(searchParams.SearchTerm))
        {
            // in DbInitializer class we defined some indexes. the search term could be based on those. For example "Ford" and "Ford black" 
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }
        //enable ordering filter
        query = searchParams.OrderBy switch
        {
            "make" => query.Sort(x=> x.Ascending(a=> a.Make)), // if search by make
            "new" => query.Sort(x=> x.Descending(a=> a.CreatedAt)), // if search by new
            _ => query.Sort(x => x.Ascending(a => a.AuctionEnd)) // default -> auction ending soonest
        };
        //enabling filter
        query = searchParams.FilterBy switch
        {
            "finished" => query.Match(x=> x.AuctionEnd < DateTime.UtcNow),
            "endingSoon" => query.Match(x=> x.AuctionEnd < DateTime.UtcNow.AddHours(6) 
            && x.AuctionEnd> DateTime.UtcNow),
            _=> query.Match(x => x.AuctionEnd > DateTime.UtcNow) 
        };
        // another filter, if user sent seller info
        if(!string.IsNullOrEmpty(searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller);
        }
        // another filter, if user sent winner info
        if(!string.IsNullOrEmpty(searchParams.Winner))
        {
            query.Match(x => x.Winner == searchParams.Winner);
        }
        // enable pagination
        query.PageNumber(searchParams.PageNumber);
        query.PageSize(searchParams.PageSize);

        var result = await query.ExecuteAsync();
        return Ok(new{
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}
