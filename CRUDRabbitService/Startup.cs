using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CRUDService.Extensions;
using CRUDService.Model;
using CRUDService.Services.Implementations;
using CRUDService.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using CRUDService.Infrastructure;
using CRUDService.Services.Interfaces;
using Autofac;
using Microsoft.Extensions.Hosting;
using CRUDService.Services;
using MassTransit;
using Autofac.Core;

namespace CRUDService
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private ILoggerFactory _loggerFactory { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var containerBuilder = new ContainerBuilder();

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            }).AddFluentValidation().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            ConfigureMassTransit(services, containerBuilder);

            services.RegisterServices(Configuration["Database:ConnectionString"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        private void ConfigureMassTransit(IServiceCollection services, ContainerBuilder containerBuilder)
        {
            services.AddScoped<IHostedService, MassTransitHostedService>();
            //services.AddScoped<UserCheckoutAcceptedIntegrationEventHandler>();

            containerBuilder.Register(c =>
            {
                var busControl = Bus.Factory.CreateUsingRabbitMq(sbc =>
                {
                    var host = sbc.Host(new Uri(Configuration["EventBus:Uri"]), h =>
                    {
                        h.Username(Configuration["EventBus:Username"]);
                        h.Password(Configuration["EventBus:Password"]);
                    });
                    /*sbc.ReceiveEndpoint(host, "basket_checkout_queue", e =>
                    {
                        e.Consumer<UserCheckoutAcceptedIntegrationEventHandler>(c);
                    });*/
                    sbc.UseExtensionsLogging(_loggerFactory);
                });
                var consumeObserver = new ConsumeObserver(_loggerFactory.CreateLogger<ConsumeObserver>());
                busControl.ConnectConsumeObserver(consumeObserver);

                var sendObserver = new SendObserver(_loggerFactory.CreateLogger<SendObserver>());
                busControl.ConnectSendObserver(sendObserver);

                return busControl;
            })
            .As<IBusControl>()
            .As<IPublishEndpoint>()
            .SingleInstance();
        }
    }
}
