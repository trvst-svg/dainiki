namespace dainiki.Components.Services;

using Microsoft.EntityFrameworkCore;

public class DainikiDbContext : DbContext
{
    public DainikiDbContext(DbContextOptions<DainikiDbContext> options) : base(options)
    {
        
    }
}
