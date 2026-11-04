using Accounts.Application;
using Accounts.Domain;
using Accounts.Infrastructure;
using Accounts.Web;
using Events.Application;
using Events.Domain;
using Events.Infrastructure;
using Events.Web;
using Admission.Application;
using Admission.Domain;
using Admission.Infrastructure;
using Admission.Web;
using Sales.Application;
using Sales.Domain;
using Sales.Infrastructure;
using Sales.Web;

var builder = WebApplication.CreateBuilder(args);

// One block per bounded context, in dependency order (Domain → Application → Infrastructure → Web).
builder
    .Services
    .AddAccountsDomain()
    .AddAccountsApplication(builder.Configuration)
    .AddAccountsInfrastructure(builder.Configuration)
    .AddAccountsWebComponents()
    .AddEventsDomain()
    .AddEventsApplication(builder.Configuration)
    .AddEventsInfrastructure(builder.Configuration)
    .AddEventsWebComponents()
    .AddSalesDomain()
    .AddSalesApplication(builder.Configuration)
    .AddSalesInfrastructure(builder.Configuration)
    .AddSalesWebComponents()
    .AddAdmissionDomain()
    .AddAdmissionApplication(builder.Configuration)
    .AddAdmissionInfrastructure(builder.Configuration)
    .AddAdmissionWebComponents();

builder.Services
    .AddSingleton(TimeProvider.System)
    .AddHttpContextAccessor()
    .AddEventSourcing()
    .AddTokenAuthentication(builder.Configuration) // ADR-001
    .AddAuthorization()
    .AddModelBinders()
    .AddExceptionHandling()
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "TecknoworksTickets API", Version = "v1" });

        // Several contexts have a type called ...Response; qualify them so Swagger tells them apart.
        c.CustomSchemaIds(type => type.FullName!.Replace('+', '.'));
    })
    .AddHttpClient();

var app = builder.Build();

app
    .UseWebService(app.Environment)
    .Initialize();

app.Run();
