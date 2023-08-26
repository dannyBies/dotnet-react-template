
using Example.Messaging.SignalR;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Example.Messaging.MassTransit;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Serialization;
using MassTransit.SignalR;

namespace Example
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .CreateLogger();

            try
            {
                var entryAssembly = Assembly.GetEntryAssembly();

                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog();

                // Add services to the container.
                builder.Services.AddSignalR();
                builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();

                builder.Services.AddValidatorsFromAssembly(entryAssembly);

                builder.Services.AddMassTransit(x =>
                {
                    x.AddSignalRHub<ExampleHub>();

                    x.AddConsumers(entryAssembly);

                    x.SetKebabCaseEndpointNameFormatter();
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.UseMessageRetry(r =>
                        {
                            r.Ignore<InvalidOperationException>();
                            r.Intervals(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));
                        });

                        cfg.Host("localhost", "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                });

                builder.Services.AddControllers(options =>
                {
                    options.Filters.Add<MassTransitExceptionFilter>();
                }).AddJsonOptions(x =>
                {
                    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    x.AllowInputFormatterExceptionMessages = false;
                });

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(options =>
                {
                    options.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Name = "Bearer",
                                In = ParameterLocation.Header,
                                Reference = new OpenApiReference
                                {
                                    Id = "Bearer",
                                    Type = ReferenceType.SecurityScheme
                                }
                            },
                            new List<string>()
                        }
                    });
                });
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddReceiveObserver<LoggingReceiveObserver>();
                builder.Services.AddPublishObserver<HttpPublishObserver>();

                builder.Services.AddCors(policyBuilder => policyBuilder
                    .AddDefaultPolicy(policy =>
                    {
                        policy.WithOrigins(new[] { "http://localhost:3000" });
                        policy.AllowCredentials();
                        policy.AllowAnyHeader();
                    }));


                var domain = $"https://{builder.Configuration["Auth0:Domain"]}/";
                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options => {
                        options.Authority = domain;
                        options.Audience = builder.Configuration["Auth0:Audience"];
                        options.TokenValidationParameters = new TokenValidationParameters
                        {

                            NameClaimType = ClaimTypes.NameIdentifier
                        };

                        // https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-7.0#built-in-jwt-authentication
                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var accessToken = context.Request.Query["access_token"];
                                var path = context.HttpContext.Request.Path;
                                if (!string.IsNullOrEmpty(accessToken) &&
                                    (path.StartsWithSegments("/hub")))
                                {
                                    context.Token = accessToken;
                                }
                                return Task.CompletedTask;
                            }
                        };
                    });

                var app = builder.Build();

                app.UseCors();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();
                app.MapHub<ExampleHub>("/hub");

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }
    }
}