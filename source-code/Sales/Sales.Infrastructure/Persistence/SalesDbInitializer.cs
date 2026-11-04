namespace Sales.Infrastructure.Persistence;

internal class SalesDbInitializer : DbInitializer
{
    public SalesDbInitializer(SalesDbContext db) : base(db)
    {
    }
}
