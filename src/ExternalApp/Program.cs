using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var jwtString = "{\"additionalData\":{},\"alg\":null,\"crv\":null,\"d\":null,\"dp\":null,\"dq\":null,\"e\":\"AQAB\",\"k\":null,\"keyId\":null,\"keyOps\":[],\"kid\":null,\"kty\":\"RSA\",\"n\":\"rqvxmKsMEUuBwgtzW1FRn29Jn0wBLNHNY5E0fsKOks2ly8b4bW2Qb7KQgYIo6xTHN0k1RisLqPKJ4XpHIw0OBoumHPTFag6UzvsQUVkT6_H9rPKTMjNi1tfQ_YNN77FoWXQymp89oH1bD0C5TGygfCOlJzj0l1yLDVGrF5uD4jI5LVcfhZw9ziclwCLPQMwwqUz-GanZMzF4XbldyVHmbABIIuzpZvtAnrIPV9U3sl-NblNpVbDHoylp_hYr6iU1Q-pk51x-H3vH1O72YxJ6zS_MbU7VJAo99ie89ytZd6Ukp2CBGEFR46JDpzMcyhHfKR66vYDIOfHaH8_um8obFQ\",\"oth\":null,\"p\":null,\"q\":null,\"qi\":null,\"use\":null,\"x\":null,\"x5c\":[],\"x5t\":null,\"x5tS256\":null,\"x5u\":null,\"y\":null,\"keySize\":2048,\"hasPrivateKey\":false,\"cryptoProviderFactory\":{\"cryptoProviderCache\":{},\"customCryptoProvider\":null,\"cacheSignatureProviders\":true,\"signatureProviderObjectPoolCacheSize\":16}}";

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
                JsonWebKey.Create(jwtString),
            }
        };
        o.MapInboundClaims = false;
    });

var app = builder.Build();

app.UseAuthentication();

app.MapGet("/", (HttpContext ctx) => ctx.User.FindFirst("sub")?.Value ?? "empty");

app.Run();
