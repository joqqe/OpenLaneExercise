using MassTransit;
using MassTransit.Middleware;
using OpenLane.Common.Extensions;
using OpenLane.Domain.Messages;
using OpenLane.Infrastructure;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
	.ConfigureResource(resource => resource.AddService("OpenLane.MessageProcessor"))
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

builder.Services.AddHealthChecks();

builder.Services.AddMassTransit(config =>
{
	config.SetKebabCaseEndpointNameFormatter();

	config.AddConsumers(typeof(Program).Assembly);

	config.AddConfigureEndpointsCallback((context, name, cfg) =>
	{
		if (name.Equals("bid-received"))
		{
			var partitioner = cfg.CreatePartitioner(10);
			cfg.UsePartitioner<BidReceivedMessage>(partitioner, m => m.Message.OfferObjectId);
		}
	});

	config.UsingRabbitMq((ctx, cfg) =>
	{
		cfg.Host(builder.Configuration.GetConnectionString("MessageQueue"));
		cfg.ConfigureEndpoints(ctx);
		cfg.UseInMemoryOutbox(ctx);
		cfg.Durable = true;
		cfg.UseMessageRetry(r => r.Incremental(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10)));
		cfg.UseRateLimit(20, TimeSpan.FromSeconds(1));
	});
});

builder.Services.AddInfra(builder.Configuration);
builder.Services.AddHandlers(typeof(Program).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/api/health");

app.Run();

// For testing purposes
public partial class Program { }
