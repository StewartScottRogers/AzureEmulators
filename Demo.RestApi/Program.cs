using Demo.RestApi.Services;

WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder(args);

// Set the URL to match Docker configuration
webApplicationBuilder.WebHost.UseUrls("http://0.0.0.0:5000");

// Add services to the container.
webApplicationBuilder.Services.AddControllers();
webApplicationBuilder.Services.AddOpenApi();

// Add Service Bus messaging service
webApplicationBuilder.Services.AddSingleton<ServiceBusMessageService>();

WebApplication webApplication = webApplicationBuilder.Build();

// Configure the HTTP request pipeline.
webApplication.MapOpenApi();

webApplication.UseHttpsRedirection();

webApplication.MapControllers();

webApplication.Run();
