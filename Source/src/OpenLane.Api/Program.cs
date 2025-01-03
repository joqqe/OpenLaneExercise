using OpenLane.Api.Application.Bids;
using OpenLane.Api.Common.Exceptions;
using OpenLane.Api.Infrastructure;
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

builder.Services.AddSignalR();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddMassTransit(config =>
{
	config.SetKebabCaseEndpointNameFormatter();

	config.AddConsumers(typeof(Program).Assembly);

	config.UsingRabbitMq((ctx, cfg) =>
	{
		var host = builder.Configuration.GetValue<string>("MessageQueue:Host");
		var username = builder.Configuration.GetValue<string>("MessageQueue:Username");
		var password = builder.Configuration.GetValue<string>("MessageQueue:Password");

		cfg.Host(host, "/", h =>
		{
			h.Username(username!);
			h.Password(password!);
		});

		cfg.ConfigureEndpoints(ctx);
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

app.UseHttpsRedirection();
app.UseBids();

app.Run();

// For testing purposes
public partial class Program { }