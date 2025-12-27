var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<MedicalDeviceApi.Interfaces.IDeviceRepository, MedicalDeviceApi.Repositories.DeviceRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

// Add request logging middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation($"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
    await next();
    logger.LogInformation($"Response: {context.Response.StatusCode}");
});

app.MapControllers();

app.Run();
