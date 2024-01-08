using System.Security.Claims;
using IdentityModel;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityService;

public class SeedData
{
    
    public static void EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        // adding the db context from scope
        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
        // then we migrate the db
        context.Database.Migrate();
        // we get the user manager service from netcore identity, because netcore identity is responsible for user management not identity server
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        // if there are users in db, no need to seed the db with users
        if(userMgr.Users.Any()) return;

        // now that we have the user manager, we can add new users
        var alice = userMgr.FindByNameAsync("alice").Result;
        // if we don't have a user with name alice, then create one
        if (alice == null)
        {
            alice = new ApplicationUser
            {
                UserName = "alice",
                Email = "AliceSmith@email.com",
                EmailConfirmed = true,
            };
            // we create the user with hardcoded password
            var result = userMgr.CreateAsync(alice, "Pass123$").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            // if successful, then we create some claims for the user
            // claims are what users say about themselves
            // you can add claims for Name,GivenName, familyname, website etc, but we don't need any of them but the name
            result = userMgr.AddClaimsAsync(alice, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Alice Smith")
                        }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("alice created");
        }
        else
        {
            Log.Debug("alice already exists");
        }

        var bob = userMgr.FindByNameAsync("bob").Result;
        if (bob == null)
        {
            bob = new ApplicationUser
            {
                UserName = "bob",
                Email = "BobSmith@email.com",
                EmailConfirmed = true
            };
            var result = userMgr.CreateAsync(bob, "Pass123$").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(bob, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Bob Smith")
                        }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("bob created");
        }
        else
        {
            Log.Debug("bob already exists");
        }
    }
}
