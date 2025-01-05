using OpenLane.Api.Application.Bids;
using OpenLane.Api.Common.Exceptions;
using OpenLane.Api.Infrastructure;
using MassTransit;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using FluentValidation;
using OpenLane.Domain;

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

#region SeedDatabase
// Todo: should be remove when missing endpoints are implementend like, post product and post offer!
if (builder.Environment.IsDevelopment())
{
#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
	using var sp = builder.Services.BuildServiceProvider();
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
	using var scope = sp.CreateScope();
	var scopedServices = scope.ServiceProvider;
	var appDbContext = scopedServices.GetRequiredService<AppDbContext>();

	appDbContext.Database.EnsureDeleted();
	appDbContext.Database.EnsureCreated();

	var offerObjectId = Guid.Parse("57e3f9d5-a32c-4d9a-94cb-79a3fea2368a");
	var productA = new Product
	{
		ObjectId = Guid.NewGuid(),
		Name = "ProductA"
	};
	var openOffer = new Offer
	{
		ObjectId = offerObjectId,
		Product = productA,
		StartingPrice = 100m,
		OpensAt = DateTimeOffset.Now,
		ClosesAt = DateTimeOffset.Now.AddMonths(1)
	};
	var newBid = new Bid
	{
		ObjectId = Guid.NewGuid(),
		Offer = openOffer,
		Price = 110m,
		ReceivedAt = DateTimeOffset.Now,
		UserObjectId = Guid.NewGuid()
	};
	appDbContext.Bids.Add(newBid);
	appDbContext.SaveChanges();
}
#endregion

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
