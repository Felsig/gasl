using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using gasl.Mvc;
using gasl.Web.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using gasl.Web.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using gasl.Infrastructure.Data;

namespace gasl.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env) 
        {
            CurrentEnvironment = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
            .AddJsonFile("conf/appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"conf/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            builder.AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            Configuration = builder.Build();    
        }

        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment CurrentEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDbContext<UserContext>(options =>
                    options.UseInMemoryDatabase())
                .AddDbContext<LinkContext>(options =>
                    options.UseInMemoryDatabase());

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<UserContext>()
                .AddDefaultTokenProviders();

            services.AddMvcWithFeatureRouting();

            services.AddSingleton(_ => Configuration);

            services.AddScoped<LinkRepository>();

            services.AddTransient<SeedData>();

        }

        public async void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory,
            SeedData seedData
        )
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseCookieAuthentication(GetCookieAuthenticationConfiguration());

            app.UseMvc(ConfigureRoutes);

            await seedData.InitializeAsync();

        }

        private static CookieAuthenticationOptions GetCookieAuthenticationConfiguration()
        {
            return new CookieAuthenticationOptions()
            {
                AuthenticationScheme = "Cookie",
                LoginPath = new PathString("/account/login"),
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            };
        }

        private static void ConfigureRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapRoute(
                name: "default",
                template: "{controller=home}/{action=index}/{id?}");
        }
    }
}
