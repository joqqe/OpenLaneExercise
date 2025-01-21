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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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

builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		var secret = builder.Configuration.GetValue<string>("Authentication:Secret")!;
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

		options.RequireHttpsMetadata = false;
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidIssuer = builder.Configuration.GetValue<string>("Authentication:Issuer"),
			ValidateIssuer = true,
			
			ValidAudience = builder.Configuration.GetValue<string>("Authentication:Audience"),
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
