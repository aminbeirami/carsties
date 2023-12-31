using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDbContext : DbContext // inherit from entity framework class
{
    public AuctionDbContext(DbContextOptions options) : base(options)
    {   
    }

    public DbSet<Auction> Auctions {get; set; }
}
