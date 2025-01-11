using OpenLane.Api.Application.Bids;
using OpenLane.Api.Common.Exceptions;
using OpenLane.Infrastructure;
using MassTransit;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using FluentValidation;
using OpenLane.Api.Hub;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
	.ConfigureResource(resource => resource.AddService("OpenLane.Api"))
	.WithTracing(tracing =>
	{
		tracing
			.AddAspNetCoreInstrumentation()
			.AddHttpClientInstrumentation()
			.AddEntityFrameworkCoreInstrumentation()
			.AddSource("MassTransit")
			.AddRedisInstrumentation();

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

	config.AddConsumers(typeof(Program).Assembly);

	config.AddConfigureEndpointsCallback((context, name, cfg) =>
	{
		cfg.UseMessageRetry(r => r.Incremental(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10)));
	});

	config.UsingRabbitMq((ctx, cfg) =>
	{
		cfg.Host(builder.Configuration.GetConnectionString("MessageQueue"));
		cfg.ConfigureEndpoints(ctx);
		cfg.UseRateLimit(50, TimeSpan.FromSeconds(5));
		cfg.Durable = true;
	});
});

builder.Services.AddSignalR();

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

app.UseHttpsRedirection();

app.MapHealthChecks("/api/health");
app.MapHub<NotificationHub>("/api/notification");

app.UseBids();

app.Run();

// For testing purposes
public partial class Program { }
