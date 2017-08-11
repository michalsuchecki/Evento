using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Evento.Core.Repositories;
using Evento.Infrastructure.Mappers;
using Evento.Infrastructure.Repositories;
using Evento.Infrastructure.Services;
using Evento.Infrastructure.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using NLog.Web;

namespace Evento.Api
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public IContainer Container { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }



        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc()
                .AddJsonOptions(x =>
                {
                    x.SerializerSettings.Formatting = Formatting.Indented;
                });

            services.AddAuthorization(x =>
            {
                x.AddPolicy("HasAdminRole", p => p.RequireRole("admin"));
            });

            services.AddMemoryCache();

            //services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITicketService, TicketService>();

            services.AddScoped<IDataInitializer, DataInitializer>();
            services.AddSingleton<IJwtHander, JwtHandler>();
            services.AddSingleton<IMapper>(AutoMapperConfig.Initialize());

            services.Configure<JwtSettings>(Configuration.GetSection("jwt"));
            services.Configure<AppSettings>(Configuration.GetSection("app"));

            var builder = new ContainerBuilder();

            builder.Populate(services);
            builder.RegisterType<EventRepository>().As<IEventRepository>().InstancePerLifetimeScope();
           // builder.RegisterType<DataInitializer>().As<IDataInitializer>().InstancePerLifetimeScope();

            Container = builder.Build();

            return new AutofacServiceProvider(Container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            // loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            // loggerFactory.AddDebug();
            loggerFactory.AddNLog();
            app.AddNLogWeb();
            env.ConfigureNLog("nlog.config");

            var jwtSettings = app.ApplicationServices.GetService<IOptions<JwtSettings>>();

            app.UseJwtBearerAuthentication(new JwtBearerOptions()
            {
                AutomaticAuthenticate = true,
                TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = false,
                    ValidIssuer = jwtSettings.Value.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Key))
                }
            });
            app.UseMvc();

            SeedData(app);

            appLifetime.ApplicationStopped.Register(() =>
            {
                Container.Dispose();
            });
        }

        private void SeedData(IApplicationBuilder appBuilder)
        {
            var settings = appBuilder.ApplicationServices.GetService<IOptions<AppSettings>>();
            if(settings.Value.SeedData)
            {
                var initializer = appBuilder.ApplicationServices.GetService<IDataInitializer>();
                initializer.SeedAsync();
            }
        }
    }
}
