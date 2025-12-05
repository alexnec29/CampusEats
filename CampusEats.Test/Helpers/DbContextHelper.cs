using CampusEats.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Test.Helpers;

public class DbContextHelper
{
    public static CampusEatsDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CampusEatsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        CampusEatsDbContext dbContext = new CampusEatsDbContext(options);
        return dbContext;
    }
}