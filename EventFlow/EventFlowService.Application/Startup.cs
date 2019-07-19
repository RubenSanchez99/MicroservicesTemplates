using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using EventFlow;
using EventFlow.AspNetCore.Middlewares;
using EventFlow.Autofac.Extensions;
using EventFlow.Extensions;
using EventFlowService.Application.Infrastructure.AggregatesContext;
using EventFlowService.Application.Infrastructure.RepositoryContext;
using EventFlowService.Application.Services;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EventFlowService.Application
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        private ILoggerFactory _loggerFactory { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var containerBuilder = new ContainerBuilder();

            ConfigureDbContext(services);

            ConfigureEventFlow(services, containerBuilder);

            ConfigureMassTransit(services, containerBuilder);

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            }).AddFluentValidation().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            containerBuilder.Populate(services);

            return new AutofacServiceProvider(containerBuilder.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<CommandPublishMiddleware>();

            app.UseMvc();
        }

        private void ConfigureDbContext(IServiceCollection services)
        {
            services.AddEntityFrameworkNpgsql();

            services.AddDbContext<AggregatesContext>(options =>
            {
                options.UseNpgsql(Configuration["Database:WriteModelConnectionString"],
                                     npgsqlOptionsAction: sqlOptions =>
                                     {
                                         sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                                         //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                                         sqlOptions.EnableRetryOnFailure(maxRetryCount: 5);
                                     });
            });

            services.AddDbContext<RepositoryContext>(options =>
            {
                options.UseNpgsql(Configuration["Database:RepositoryConnectionString"],
                                     npgsqlOptionsAction: sqlOptions =>
                                     {
                                         sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                                         //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                                         sqlOptions.EnableRetryOnFailure(maxRetryCount: 5);
                                     });
            });

            /*services.AddDbContext<ReadModelContext>(options =>
            {
                options.UseNpgsql(Configuration["Database:ReadModelConnectionString"],
                                     npgsqlOptionsAction: sqlOptions =>
                                     {
                                         sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                                         //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                                         sqlOptions.EnableRetryOnFailure(maxRetryCount: 5);
                                     });
            });*/
        }

        private void ConfigureEventFlow(IServiceCollection services, ContainerBuilder containerBuilder)
        {
            var events = new List<Type>()
            {
                //typeof(ExampleDomainEvent)
            };

            var commandHandlers = new List<Type>
            {
                //typeof(ExampleCommandHandler)
            };

            var container = EventFlowOptions.New
                .UseAutofacContainerBuilder(containerBuilder)
                .UseConsoleLog()
                .AddEvents(events)
                .AddCommandHandlers(commandHandlers)
                .AddSubscribers(typeof(Startup).Assembly)
                //.AddEntityFrameworkReadModel()
                ;
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
                    });
                    sbc.ReceiveEndpoint(host, "stock_confirmed_queue", e =>
                    {
                    });
                    sbc.ReceiveEndpoint(host, "order_validation_state", e =>
                    {
                        e.UseRetry(x =>
                        {
                            x.Handle<DbUpdateConcurrencyException>();
                            x.Interval(5, TimeSpan.FromMilliseconds(100));
                        }); // Add the retry middleware for optimistic concurrency
                        e.StateMachineSaga(new GracePeriodStateMachine(c.Resolve<IAggregateStore>()), new InMemorySagaRepository<GracePeriod>());
                    });*/
                    sbc.UseExtensionsLogging(_loggerFactory);
                    sbc.UseInMemoryScheduler();
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

        private void ConfigureValidators(IServiceCollection services)
        {
        }
    }
}
