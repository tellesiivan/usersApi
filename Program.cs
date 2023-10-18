using DotnetApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS Support
builder.Services.AddCors(
    (options) =>
    {
        // Add a cors policy for dev env
        options.AddPolicy(
            "DevCors",
            (corsBuilder) =>
            {
                // set the urls we want to access our endpoints
                corsBuilder
                    .WithOrigins(
                        "http://localhost:4200",
                        "http://localhost:3000",
                        "http://localhost:8000"
                    )
                    // Allow all of the above endpoints to have access to the following
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        );
        // Add a cors policy for prod env
        options.AddPolicy(
            "ProdCors",
            (corsBuilder) =>
            {
                // set the urls we want to access our endpoints
                corsBuilder
                    .WithOrigins("https://tellesiivan.com")
                    // Allow all of the above endpoints to have access to the following
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        );
    }
);

// allows IUserRepsository to be used and call actions in UserRepository
// Needs to be before the app builds(builder.Build())
builder.Services.AddScoped<IUserRepsository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
