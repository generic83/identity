using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.IdentityModel.Tokens;
using WeatherMVC.Services;
using IdentityModel;

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

            }).AddCookie("cookie")
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = Configuration["InteractiveServiceSettings:AuthorityUrl"];
                options.ClientId = Configuration["InteractiveServiceSettings:ClientId"];
                options.ClientSecret = Configuration["InteractiveServiceSettings:ClientSecret"];

                //You need this to get the role claims
                options.GetClaimsFromUserInfoEndpoint = true;

                options.ResponseType = "code";
                options.UsePkce = true;
                options.ResponseMode = "query";
                //options.Scope.Add(Configuration["InteractiveServiceSettings:Scopes:0"]);
                
                //You need this to get role scope
                options.Scope.Add("role");
                options.SaveTokens = true;

                // you need this to add JWT roles as claims to ClaimsIdentity
                options.ClaimActions.Add(new JsonKeyClaimAction(JwtClaimTypes.Role, JwtClaimTypes.Role, JwtClaimTypes.Role));

                // you need this for the Authorize attribule with roles to work
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
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
