using ApiGateway.Hubs;
using ApiGateway.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IPostStreamService, PostStreamService>(); //Creates single instance for entire application lifetime

builder.Services.AddHostedService<PostStreamBackgroundService>();

// Add CORS
builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")  //access to domain  
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();  // Needed for SignalR
    });
});

var app = builder.Build();

// Use CORS before other middleware
app.UseCors("AllowFrontend");

app.UseRouting();
app.MapControllers();
app.MapHub<PostHub>("/postHub");

app.Run();

// Background service class
public class PostStreamBackgroundService : BackgroundService
{
    private readonly IPostStreamService _streamService;
    
    //Allows easy swapping of service implementations

    public PostStreamBackgroundService(IPostStreamService streamService)
    {
        _streamService = streamService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _streamService.StartListeningAsync(stoppingToken); //Starts the background service
        //Starts listening to Redis channel
    }
}