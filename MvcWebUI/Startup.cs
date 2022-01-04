using System;
using AppCore.Business.Utils;
using AppCore.Business.Utils.Bases;
using AppCore.DataAccess.Configs;
using Business.Services;
using Business.Services.Bases;
using DataAccess.EntityFramework.Contexts;
using DataAccess.EntityFramework.Repositories;
using DataAccess.EntityFramework.Repositories.Bases;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvcWebUI.Settings;

namespace MvcWebUI
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

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(config =>
                {
                    config.LoginPath = "/Accounts/Login";
                    config.AccessDeniedPath = "/Accounts/AccessDenied";
                    config.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    config.SlidingExpiration = true;
                });

            //services.AddSession();
            services.AddSession(config =>
            {
                config.IdleTimeout = TimeSpan.FromMinutes(30); // default: 20 dakika
            });

            #region IoC Container
            // IoC Container k�t�phaneleri: Autofac ve Ninject
            // services.AddScoped() // istek (request) boyunca objenin referans�n� (genelde interface veya abstract class) kulland���m�z yerde obje (somut class'tan olu�turulacak) bir kere olu�turulur ve yan�t (response) d�nene kadar bu obje hayatta kal�r.
            // services.AddSingleton() // web uygulamas� ba�lad���nda objenin referans�n� (genelde interface veya abstract class) kulland���m�z yerde obje (somut class'tan olu�uturulacak) bir kere olu�turulur ve uygulama �al��t��� (IIS �zerinden uygulama durdurulmad��� veya yeniden ba�lat�lmad���) s�rece bu obje hayatta kal�r.
            // services.AddTransient() // istek (request) ba��ms�z ihtiya� olan objenin referans�n� (genelde interface veya abstract class) kulland���m�z her yerde bu objeyi new'ler.

            ConnectionConfig.ConnectionString = Configuration.GetConnectionString("ETradeContext");

            // Unable to resolve service hatalar� burada giderilir!
            services.AddScoped<DbContext, ETradeContext>();
            services.AddScoped<ProductRepositoryBase, ProductRepository>();
            services.AddScoped<CategoryRepositoryBase, CategoryRepository>();
            services.AddScoped<UserRepositoryBase, UserRepository>();
            services.AddScoped<CountryRepositoryBase, CountryRepository>();
            services.AddScoped<CityRepositoryBase, CityRepository>();
            services.AddScoped<RoleRepositoryBase, RoleRepository>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAccountService, AccountService>();

            // ASP.NET Core kullan�m�:
            //IConfigurationSection section = Configuration.GetSection("AppSettings");
            //AppSettings appSettings = new AppSettings();
            //section.Bind(appSettings);

            // AppCore �zerinden kullan�m:
            AppSettingsUtilBase appSettingsUtil = new AppSettingsUtil(Configuration);
            appSettingsUtil.Bind<AppSettings>();
            #endregion
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

            // Sen kimsin?
            app.UseAuthentication();

            // Sen i�lem i�in yetkili misin?
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
