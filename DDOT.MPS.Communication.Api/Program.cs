using DDOT.MPS.Communication.Api;
using DDOT.MPS.Communication.Api.Managers;
using DDOT.MPS.Communication.Api.Middlewares;
using DDOT.MPS.Communication.Api.SignalrHubs;
using DDOT.MPS.Communication.Api.Telemetry;
using DDOT.MPS.Communication.Model.Configurations;
using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DdotMpsMeetingConfigeration>(builder.Configuration.GetSection(DdotMpsMeetingConfigeration.SectionName));
builder.Services.AddSignalR().AddAzureSignalR(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("AzureSignalR");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(
                "https://admin.ddot-mps-dev.codicetech.net",
                "https://admin.ddot-mps-test.codicetech.net",
                "http://localhost:3000"
            );
    });
});

//Application Insights
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddSingleton<ITelemetryInitializer, DdotMpsTelemetryInitializer>();

builder.Services.AddSingleton<IGraphClientService, GraphClientService>();
builder.Services.AddScoped<IRealTimeDataManager, RealTimeDataManager>();
builder.Services.AddScoped<IMeetingManager, MeetingManager>();

// Register the environment name as a singleton service
builder.Services.AddSingleton(builder.Environment.EnvironmentName);
// Register custom health check
builder.Services.AddHealthChecks().AddCheck<CustomHealthCheck>("custom_health_check");

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapHealthChecks("/api/v1/healthcheck");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();

app.Run();
