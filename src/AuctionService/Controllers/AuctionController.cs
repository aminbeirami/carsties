using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase // Controller base is a base class for MVC without view
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndPoint;
    public AuctionController(AuctionDbContext context,IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndPoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
        var auctions = await _context.Auctions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();
        return Ok(_mapper.Map<List<AuctionDto>>(auctions));
    }

    [HttpGet("{Id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x=> x.Id == id);
        if(auction == null)
        {
            return NotFound();
        }
        return _mapper.Map<AuctionDto>(auction);
    }
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        //ToDo.. Add current user as seller
        auction.Seller = "Test";
        _context.Auctions.Add(auction);

        // deliver the request to rabbitMQ
        // if it fail, it will be stored in outbox in postgres
        var newAuction = _mapper.Map<AuctionDto>(auction);
        await _publishEndPoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var result = await _context.SaveChangesAsync() > 0; // save changes async will return an integer. in case of failure it returns 0
        if(!result) return BadRequest("Could not write to DB");
        // we should create 201 response which CreatedAtAction does it. we need to reuse GetAuctionById and we pass the Id. and we also pass the auctionDto as a pattern that response should look like
        return CreatedAtAction(nameof(GetAuctionById), 
            new {auction.Id},newAuction);
    }

    [HttpPut("{Id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        //get it from db
        var auction = await _context.Auctions.Include(x=>x.Item)
                        .FirstOrDefaultAsync(x => x.Id == id);
        if(auction == null) return NotFound();

        //TO DO -Check seller == username
        // Update the item by the users request. if not provided by user, keep the old one
        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0;
        if(result) return Ok();
        return BadRequest(); 
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);
        if(auction == null) return NotFound();
        // To Do -- Check the Seller == username
        _context.Auctions.Remove(auction);

        var result = await _context.SaveChangesAsync() > 0;
        if(!result) return BadRequest("Could not update DB");

        return Ok();


    }

}
