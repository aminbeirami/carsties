using AuctionService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDbContext : DbContext // inherit from entity framework class
{
    public AuctionDbContext(DbContextOptions options) : base(options)
    {   
    }

    public DbSet<Auction> Auctions {get; set; }

    // we ovverride the onModelCreating of DbContext of entity framework to set up outbox for when message fail to be delievered to service bus
    // Note - since we have added this after initialization migration, we need to create another migration.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

    }
}
