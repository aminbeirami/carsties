using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionService.Entities;
[Table("Items")] // entity framework will pick this up 
public class Item
{
public Guid Id { get; set; }
public string Make { get; set; }
public string Model { get; set; }
public int Year {get; set;}
public string Color { get; set; }
public int Mileage { get; set; }
public string ImageUrl { get; set; }

//nav properties
public Auction auction { get; set; }
public Guid AuctionId {get; set;}

}
