using OpenLane.Api.Application.Bids;
using OpenLane.Api.Common.Exceptions;
using OpenLane.Infrastructure;
using MassTransit;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
	.ConfigureResource(resource => resource.AddService("OpenLane.Api"))
	.WithTracing(tracing =>
	{
		tracing
			.AddAspNetCoreInstrumentation()
			.AddHttpClientInstrumentation()
			.AddEntityFrameworkCoreInstrumentation()
			.AddMassTransitInstrumentation();

		tracing.AddOtlpExporter();
	})
	.WithMetrics(metrics =>
	{
		metrics
			.AddAspNetCoreInstrumentation()
			.AddHttpClientInstrumentation();

		metrics.AddOtlpExporter();
	});
builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddHealthChecks();

builder.Services.AddMassTransit(config =>
{
	config.SetKebabCaseEndpointNameFormatter();

	config.UsingRabbitMq((ctx, cfg) =>
	{
		cfg.Host(builder.Configuration.GetConnectionString("MessageQueue"));
		cfg.ConfigureEndpoints(ctx);
		cfg.Durable = true;
	});
});

builder.Services.AddInfra(builder.Configuration);
builder.Services.AddBids();

// ------------------------

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapHealthChecks("/api/health");

app.UseHttpsRedirection();
app.UseBids();

app.Run();

// For testing purposes
public partial class Program { }
