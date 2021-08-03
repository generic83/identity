using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ids
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityServer(
                //This sets IdentityServer cookies lifetime and sliding expiration. When they expire, and when the authentication ticket expires in the client(ASP.Net Core MVC), then the user will be prompted to enter their credentials
                //options =>
                //{
                //    options.Authentication.CookieLifetime = TimeSpan.FromSeconds(20);
                //    options.Authentication.CookieSlidingExpiration = false;
                //}
            )
                            .AddInMemoryClients(Config.Clients)
                            .AddInMemoryIdentityResources(Config.IdentityResources)
                            .AddInMemoryApiResources(Config.ApiResources)
                            .AddInMemoryApiScopes(Config.ApiScopes)
                            .AddTestUsers(Config.Users)
                            .AddDeveloperSigningCredential();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
        }
    }
}
