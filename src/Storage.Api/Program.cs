using Serilog;
using Storage.Api.DependencyInjection;
using Storage.Api.Extensions;
using Storage.Application.DependencyInjection;
using Storage.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi();

var app = builder.Build();
app.UseApi();
app.Run();

public partial class Program;