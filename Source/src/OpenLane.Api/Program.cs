using OpenLane.Api.Common.Exceptions;
using OpenLane.Infrastructure;
using MassTransit;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using FluentValidation;
using OpenLane.Api.Hub;
using OpenLane.Api.Common.Middleware;
using OpenLane.Common.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

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

builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		var secret = "your_very_long_secret_key_that_is_at_least_32_characters_long";
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

		options.RequireHttpsMetadata = false;
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidIssuer = "yourIssuer",
			ValidateIssuer = true,
			
			ValidAudience = "yourAudience",
			ValidateAudience = true,
			
			IssuerSigningKey = key,
			ValidateIssuerSigningKey = true,
			
			ValidateLifetime = true
		};
		options.Validate();
	});
builder.Services.AddAuthorization();

builder.Services.AddSignalR();

builder.Services.AddInfra(builder.Configuration);

builder.Services.AddHandlers(typeof(Program).Assembly);

// ------------------------

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<IdempotencyCheckMiddleware>();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/api/health");
app.MapHub<NotificationHub>("/api/notification");

app.UseEndpoints(typeof(Program).Assembly);

app.Run();

// For testing purposes
public partial class Program { }
