public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseWebService(
        this IApplicationBuilder app,
        IWebHostEnvironment env)
        => app
            .UseExceptionHandling(env)
            .UseExceptionHandler()
            .UseHttpsRedirection()
            .UseRouting()
            .UseCors(options => options
                .WithOrigins("http://localhost:5173") // the web/ dev server
                .AllowAnyHeader()
                .AllowAnyMethod())
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(endpoints => endpoints
                .MapControllers());

    private static IApplicationBuilder UseExceptionHandling(
        this IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TecknoworksTickets API v1"));
        }

        return app;
    }

    public static IApplicationBuilder Initialize(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();

        var initializers = serviceScope.ServiceProvider.GetServices<IDbInitializer>();

        foreach (var initializer in initializers)
        {
            initializer.Initialize();
        }

        return app;
    }
}