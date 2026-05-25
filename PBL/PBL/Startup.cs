using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace PBL
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
            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromSeconds(3600);
            });
            services.AddControllersWithViews();

            services.AddSingleton<PBL.Services.FishAiService>();
            services.AddSingleton<PBL.Services.SmartLampMqttService>();
            services.AddSingleton<PBL.Services.FiwareSthCometService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Aquamarine — API IoT",
                    Version = "v1",
                    Description =
                        "API REST do projeto PBL Aquamarine (Aquário Inteligente).\n\n" +
                        "**Endpoints principais**\n" +
                        "- `GET /api/leituras` — lista leituras com filtros opcionais\n" +
                        "- `GET /api/leituras/aquario/{id}` — leituras de um aquário\n" +
                        "- `POST /api/leituras` — dispositivo IoT envia temperatura, nível de água, TDS e salinidade\n\n" +
                        "**Base URL (desenvolvimento):** `http://localhost:5000` ou porta exibida no `dotnet run`.\n\n" +
                        "**Content-Type:** `application/json` nos POST.",
                    Contact = new OpenApiContact
                    {
                        Name = "GlitterComputer — EC5",
                        Email = "contato@exemplo.edu.br"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Uso acadêmico — PBL Linguagem de Programação I"
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

                c.TagActionsBy(api =>
                {
                    var controller = api.ActionDescriptor.RouteValues["controller"];
                    return controller == "Leituras"
                        ? new[] { "Leituras IoT" }
                        : new[] { controller };
                });

                c.OrderActionsBy(apiDesc =>
                    apiDesc.RelativePath?.StartsWith("api/", StringComparison.OrdinalIgnoreCase) == true
                        ? "0_" + apiDesc.HttpMethod + "_" + apiDesc.RelativePath
                        : "1_" + apiDesc.RelativePath);
            });
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
            }
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aquamarine API v1");
                c.RoutePrefix = "swagger";
                c.DocumentTitle = "Aquamarine — Documentação da API";
                c.DefaultModelsExpandDepth(2);
                c.DisplayRequestDuration();
                c.EnableDeepLinking();
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Login}/{action=Index}/{id?}");
            });
        }
    }
}
