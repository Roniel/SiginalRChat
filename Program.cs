using SiginalRChat.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var secretKey = "fasdfasdfasdfasdfasdfasdfasdfadfasdfasdf";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // Só aplica para requisições no /chatHub
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

builder.Services.AddSingleton<IAgenteIAService, AgenteIAService>();
builder.Services.AddHttpClient<AgenteIAService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:11434"); // <- Substitua
});

var app = builder.Build();

app.UseDefaultFiles(); // index.html
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // obrigatórios
app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");

app.MapPost("/login", (UserLogin login) =>
{
   // if (!String.IsNullOrEmpty(login.Username) && String.IsNullOrEmpty(login.Password))
  ///  {
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[] {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, login.Username)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Results.Ok(new { token = tokenString });
  //  }

   // return Results.Unauthorized();
});

app.Run();

record UserLogin(string Username, string Password);