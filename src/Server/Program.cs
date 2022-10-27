using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

var rsaKey = RSA.Create();
rsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Jwt")
    .AddJwtBearer("Jwt", o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = false,
            ValidateIssuer = false,
        };

        o.Events = new JwtBearerEvents()
        {
            OnMessageReceived = (ctx) =>
            {
                if (ctx.Request.Query.ContainsKey("t"))
                {
                    ctx.Token = ctx.Request.Query["t"];
                }
                return Task.CompletedTask;
            }
        };

        o.Configuration = new OpenIdConnectConfiguration()
        {
            SigningKeys =
            {
                new RsaSecurityKey(rsaKey),
            }
        };
        o.MapInboundClaims = false;
    });
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
app.UseAuthentication();

app.MapGet("/", (HttpContext ctx) => ctx.User.FindFirst("sub")?.Value ?? "empty");

app.MapGet("/jwt", () =>
{
    var handler = new JsonWebTokenHandler();
    var key = new RsaSecurityKey(rsaKey);
    var token = handler.CreateToken(new SecurityTokenDescriptor()
    {
        Issuer = "https://localhost:5000/",
        Subject = new ClaimsIdentity(new[]
        {
            new Claim("sub",Guid.NewGuid().ToString()),
            new Claim("name","behdad"),

        }),
        SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
    });
    return token;
});

app.MapGet("/jwk", () =>
{
    var publicKey = RSA.Create();
    publicKey.ImportRSAPublicKey(rsaKey.ExportRSAPublicKey(), out _);
    var key = new RsaSecurityKey(publicKey);
    return JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
});

app.MapGet("/jwk-p", () =>
{
    var key = new RsaSecurityKey(rsaKey);
    return JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
});

app.Run();
