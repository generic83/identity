using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.IdentityModel.Tokens;
using WeatherMVC.Services;
using IdentityModel;
using System;
using System.Threading.Tasks;

namespace WeatherMVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
           
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookie";
                options.DefaultChallengeScheme = "oidc";

            }).AddCookie("cookie", options =>
            {
                // Configure the client application to use sliding sessions. True is the default
                // If you set this to false, then the cookie containing the authentication ticket will expire based on the expired time set in the OnTickectReceived event
                //options.SlidingExpiration = false;

                // Expire the session of 15 minutes of inactivity
                // options.ExpireTimeSpan = TimeSpan.FromSeconds(10);

                
                //options.Events.OnValidatePrincipal = context =>
                //{
                //    if (context.Properties.Items.ContainsKey(".Token.expires_at"))
                //    {
                //        var expire = DateTime.Parse(context.Properties.Items[".Token.expires_at"]);
                //        if (expire > DateTime.Now) //TODO:change to check expires in next 5 mintues.
                //        {
                //            //TODO: send refresh token to ASOS. Update tokens in context.Properties.Items
                //            //context.Properties.Items["Token.access_token"] = newToken;
                //            context.ShouldRenew = true;
                //        }
                //    }
                //    return Task.FromResult(0);
                //};
            })
            .AddOpenIdConnect("oidc", options =>
            {
                //This sets is the expiration time of the cookie
                options.Events.OnTicketReceived = async context =>
                {
                    //This set the authentication ticket expiry, which is the result of successful authentication against identity server, which then result in access token. The access token expiry time has nothing to do with this
                    //The authentication ticket is stored in the ASP.Net Core Web cookie
                    //When it expires, the ASP.Net Core Web app will request a new ticket, and so it tries to ask the IdentityServer for new tokens(token_id, access token, and refresh token).
                    //Now if IdentityServer cookies, idsrv, are sill valid, the authentication will happen automatically, meaning the user does not have to enter username and password, and so a new token will be sent by IdentityServer
                    //But if IdentityServer cookies have already expired, then the user will be prompted to enter username and password to get authenticated, and so get new tokens
                    //Look at commented code in IdentityServer startup where cookie settings can be configured(sliding expiration and expiration time)
                    //Setting the options.SlidingExpiration to true under the AddCookie options above will slide the expiry time for the authentication ticket;
                    //context.Properties.ExpiresUtc = DateTime.UtcNow.AddMinutes(5);

                    /* By default created cookies are Session cookies, when you close the browser, the cookies get deleted
                    If IsPersistent is set to true then they either expire or get manually deleted.
                    This will cause the cookie to persist even if you close the browser.
                    In browser dev tools, you can see Cookie Expires/Max-Age is set to session when IsPersistent is set to false, and an expiry date when IsPersistent is set to true
                    */
                    //context.Properties.IsPersistent = true;
                };

                options.Authority = Configuration["InteractiveServiceSettings:AuthorityUrl"];
                options.ClientId = Configuration["InteractiveServiceSettings:ClientId"];
                options.ClientSecret = Configuration["InteractiveServiceSettings:ClientSecret"];

                //You need this to get the role claims
                options.GetClaimsFromUserInfoEndpoint = true;

                options.ResponseType = "code";
                options.ResponseMode = "query";
                options.UsePkce = true;
                
                //you need this if you want the access token to have access to weatherapi
                options.Scope.Add(Configuration["InteractiveServiceSettings:Scopes:0"]);

                //add this scope for refresh token
                options.Scope.Add("offline_access");

                //You need this to get role scope
                options.Scope.Add("role");
                options.SaveTokens = true;

                // you need this to add JWT roles as claims to ClaimsIdentity
                options.ClaimActions.Add(new JsonKeyClaimAction(JwtClaimTypes.Role, JwtClaimTypes.Role, JwtClaimTypes.Role));
                // you need this for the Authorize attribule with roles to work
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role",
                   
                };

            });

            services.Configure<IdentityServerSettings>(Configuration.GetSection("IdentityServerSettings"));
            services.AddSingleton<ITokenService, TokenService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
