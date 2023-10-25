using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configOcelot = new ConfigurationBuilder()
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true).Build();

// Add services to the container.

builder.Services.AddOcelot(configOcelot);

// Add Authentication
string jwtSecurityKey = builder.Configuration.GetSection("Security:JwtSecrectKey").Get<string>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    byte[] key = Encoding.ASCII.GetBytes(jwtSecurityKey);

    options.TokenValidationParameters = new TokenValidationParameters {
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ValidIssuer = "",
        ValidAudience = "",
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true
    };
});

// Adding Cors
string[]? Origins = builder.Configuration["CorsConfiguration:Origins"].Split(',');
string[]? Methods = builder.Configuration["CorsConfiguration:Methods"].Split(',');

builder.Services.AddCors(options => {
    options.AddPolicy("MyPolicyJazani", builder => { builder.WithOrigins(Origins).WithMethods(Methods).AllowAnyHeader(); });
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("MyPolicyJazani");
app.UseOcelot().Wait();

app.UseAuthorization();

app.MapControllers();

app.Run();
