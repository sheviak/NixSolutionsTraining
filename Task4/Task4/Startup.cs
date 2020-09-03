using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Task4.Models;
using Task4.CustomProvider;
using Task4.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Task4
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options => 
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Home/Login");
                    options.LogoutPath = new Microsoft.AspNetCore.Http.PathString("/Home/LogOut");
                });

            services.TryAdd(ServiceDescriptor.Scoped<IUserStore<CustomUser>, CustomUserStore>());
            services.TryAdd(ServiceDescriptor.Scoped<IUserPasswordStore<CustomUser>, CustomUserStore>());
            services.TryAdd(ServiceDescriptor.Scoped<IUserClaimsPrincipalFactory<CustomUser>, CustomSecurityClaimsFactory>());
            services.AddDefaultIdentity<CustomUser>(
                options =>
                {
                    options.Password.RequiredLength = 6;
                });

            services.AddTransient<ApplicationDbContext>(e => new ApplicationDbContext(Configuration.GetConnectionString("DefaultConnection")));

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
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