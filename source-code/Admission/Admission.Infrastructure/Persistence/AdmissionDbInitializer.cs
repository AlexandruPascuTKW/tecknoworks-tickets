namespace Admission.Infrastructure.Persistence;

internal class AdmissionDbInitializer : DbInitializer
{
    public AdmissionDbInitializer(AdmissionDbContext db) : base(db)
    {
    }
}
