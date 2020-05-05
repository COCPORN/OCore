using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using OCore.Dashboard.Diagnostics.Sinks;
using OCore.Diagnostics;
using Orleans.Hosting;
using System;
using System.IO;

namespace OCore.Dashboard
{
    public static class Extensions
    {
        public static void AddOCoreDashboard(this ISiloBuilder siloBuilder)
        {
            siloBuilder.AddSink<DashboardSink>();
        }

        public static void UseOCoreDashboard(this IApplicationBuilder app, IHostEnvironment env, string requestPath = "/admin")
        {
            // TODO: Make sure these numbers make sense, these certainly seem not to
            try
            {
                var cachePeriod = env.IsDevelopment() ? "1" : "604800";
                app.UseDefaultFiles("/admin");
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "wwwroot")),
                    RequestPath = requestPath,
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
                    }
                });
            } catch (Exception ex) {
                System.Console.WriteLine($"Unable to setup Dashboard: {ex.ToString()}");
            }
        }
    }
}
