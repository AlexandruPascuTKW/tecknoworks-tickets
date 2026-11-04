var builder = WebApplication.CreateBuilder(args);

// One block per bounded context, added when the context is created from its spec:
//
// builder
//     .Services
//     .Add<Ctx>Domain()
//     .Add<Ctx>Application(builder.Configuration)
//     .Add<Ctx>Infrastructure(builder.Configuration)
//     .Add<Ctx>WebComponents();

builder.Services
    .AddHttpContextAccessor()
    .AddEventSourcing()
    .AddModelBinders()
    .AddExceptionHandling()
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "TecknoworksTickets API", Version = "v1" });
    })
    .AddHttpClient();

// Authentication: when the Access context is built, add AddTokenAuthentication(builder.Configuration)
// here and UseAuthentication()/UseAuthorization() in ApplicationBuilderExtensions.

var app = builder.Build();

app
    .UseWebService(app.Environment)
    .Initialize();

app.Run();
