﻿namespace Naos.Sample.App.Web
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ViewComponents;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.App.Commands;
    using Naos.Core.App.Configuration;
    using Naos.Core.App.Operations.Serilog;
    using Naos.Core.App.Web;
    using Naos.Core.Common.Dependency.SimpleInjector;
    using Naos.Core.Domain;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Infrastructure.Azure;
    using Naos.Sample.Countries;
    using Naos.Sample.Customers;
    using Naos.Sample.UserAccounts;
    using SimpleInjector;
    using SimpleInjector.Integration.AspNetCore.Mvc;
    using SimpleInjector.Lifestyles;

    public class Startup
    {
        private readonly Container container = new Container();

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddHttpContextAccessor()
                .AddMvc(o =>
                {
                    //o.Filters.Add(typeof(GlobalExceptionFilter)); TODO
                }).AddJsonOptions(o => o.AddDefaultJsonSerializerSettings())
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            this.IntegrateSimpleInjector(services, this.container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            this.InitializeContainer(app);
            this.container.Verify();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private void IntegrateSimpleInjector(IServiceCollection services, Container container) // TODO: move to App.Web (extension method)
        {
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IControllerActivator>(new SimpleInjectorControllerActivator(this.container));
            services.AddSingleton<IViewComponentActivator>(new SimpleInjectorViewComponentActivator(this.container));
            services.EnableSimpleInjectorCrossWiring(this.container);
            services.UseSimpleInjectorAspNetRequestScoping(this.container);
        }

        private void InitializeContainer(IApplicationBuilder app)
        {
            // Add application presentation components:
            this.container.RegisterMvcControllers(app);
            this.container.RegisterMvcViewComponents(app);

            // Naos application services.
            var configuration = NaosConfigurationFactory.CreateRoot();
            this.container
                .AddNaosMediator(new[] { typeof(IEntity).Assembly, typeof(Customers.Domain.Customer).Assembly })
                .AddNaosLogging(configuration)
                .AddNaosAppCommands(new[] { typeof(Customers.Domain.Customer).Assembly })
                .AddNaosMessaging(
                    configuration,
                    AppDomain.CurrentDomain.FriendlyName,
                    assemblies: new[] { typeof(IMessageBus).Assembly, typeof(Customers.Domain.Customer).Assembly });

            // naos sample registrations
            this.container
                .AddSampleCountries()
                .AddSampleCustomers(configuration)
                .AddSampleUserAccounts(configuration);

            // Allow Simple Injector to resolve services from ASP.NET Core.
            this.container.AutoCrossWireAspNetComponents(app);
        }
    }
}
