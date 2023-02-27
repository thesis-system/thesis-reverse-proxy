using Thesis.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

var servicesSection = builder.Configuration.GetSection(nameof(ThesisServices));
var thesisServices = servicesSection.Get<ThesisServices>();

builder.Services.AddOptions<ThesisServices>()
    .Bind(servicesSection);

builder.Services.AddReverseProxy().UseThesisServices(thesisServices.PublicUrl);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"/swagger/auth/v1/swagger.json", "Thesis.Auth v1");
    c.SwaggerEndpoint($"/swagger/requests/v1/swagger.json", "Thesis.Requests v1");
    c.SwaggerEndpoint($"/swagger/images/v1/swagger.json", "Thesis.Images v1");
});

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapReverseProxy();
});

app.Run();