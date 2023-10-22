using System.Text;
using DotnetApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                // todo: find out why the test sample key returns an empty string(ref --> AuthController L179)
                Encoding.UTF8.GetBytes("38128ewqrdbhsw=1293210348-2903hsjiadak")
            ),
            ValidateAudience = false,
            ValidateIssuer = false
        };
    });

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

// needs to be before UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
