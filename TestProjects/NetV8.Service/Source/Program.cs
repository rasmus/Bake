var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/ping", () => "Pong!");

Console.WriteLine("Hello, World!");

app.Run();
