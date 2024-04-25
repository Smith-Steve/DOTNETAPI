// This builds our API.
// It is a server that is going to run.
// The server will listen and repsond to requests.
// This is just sitting there awaiting requests.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Swagger is a tool for us to test with.
//You can use it live, but not a good idea.
// This will allow us to run the swagger UI,
// It allows us to explore the API for testing purposes.

var app = builder.Build();
//This builds the application.
// 'app' is the variable that we run.
//This is where we 'configure' the application.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //We test for the dev environment.
    //When in development, we will have access
    //To some of the additional testing features.
    
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
} 
else
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.UseHttpsRedirection();
//This allows us to send users to an HTTPS route, instead of an HTTP route.
//This is a more secure method of accessing the API.
//This can be a pain becuase during dev we don't have this.
//We'll get an error in Dev environment.

// app.MapGet("/weatherforecast", () =>
// {
// })
// .WithName("GetWeatherForecast")
// .WithOpenApi();

app.Run();