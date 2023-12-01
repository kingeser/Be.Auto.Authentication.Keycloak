using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Be.Auto.Authentication.Keycloak.Role;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages().AddMvcOptions(t =>
{
    t.Filters.Add<KeycloakAsyncAuthorizationFilter>();
});


builder.Services.AddMvcCore();
builder.Services.AddMvc();
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "Test";
        options.SlidingExpiration = true;
    })
    .AddOpenIdConnect(options =>
    {
        options.Authority = "http://localhost:8080/realms/master";
        options.ClientId = "test-client";
        options.ClientSecret = "85dFFVJ9zIghgWJQFaKKfxEIStt22vYG";
        options.ResponseType = "code";
        options.SaveTokens = false;
        options.Scope.Add("openid");
        options.UsePkce = true;
        options.CallbackPath = "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-callback-oidc";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = ClaimTypes.Role,
        };
        options.RequireHttpsMetadata = false;
    });


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();


}

if (app.Environment.IsDevelopment())
{
    app.MigrateKeycloakRoles(option =>
    {
        option.KeycloakUrl = "http://localhost:8080";
        option.AdminClientId = "admin-cli";
        option.AdminRealm = "master";
        option.AdminClientSecret = "pMlTGidScso7dQ27AFY4DXczO8Baegtr";
        option.ClientId = "test-client";
        option.ClientRealm = "master";
        option.AdminScope = null;

    });

}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

app.Run();
