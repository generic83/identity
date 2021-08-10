using ids.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace ids
{
    public class Startup
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;


            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(connectionString, opt =>
                {
                    opt.MigrationsAssembly(migrationAssembly);
                });
            });

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddIdentityServer(options =>
            {
            //This sets IdentityServer cookies lifetime and sliding expiration. When they expire, and when the authentication ticket expires in the client(ASP.Net Core MVC), then the user will be prompted to enter their credentials
            //    options.Authentication.CookieLifetime = TimeSpan.FromSeconds(20);
            //    options.Authentication.CookieSlidingExpiration = false;
            }
            )
                            //In memory 
                            //.AddInMemoryClients(Config.Clients)
                            //.AddInMemoryIdentityResources(Config.IdentityResources)
                            //.AddInMemoryApiResources(Config.ApiResources)
                            //.AddInMemoryApiScopes(Config.ApiScopes)
                            //EFCore - look in SeedData
                            .AddConfigurationStore(opt =>
                            {
                                opt.ConfigureDbContext = builder =>
                                builder.UseSqlite(connectionString, b => b.MigrationsAssembly(migrationAssembly));
                            })
                            .AddOperationalStore(opt =>
                            {
                                opt.ConfigureDbContext = builder =>
                                builder.UseSqlite(connectionString, b => b.MigrationsAssembly(migrationAssembly));
                            })
                            //.AddTestUsers(Config.Users)
                            .AddAspNetIdentity<IdentityUser>()
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
