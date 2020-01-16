using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OCore.DefaultSetup;

namespace OCore.Setup
{
    public class DeveloperStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddDefaultOCore();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseRouting();
            app.UseDefaultOCore();
        }
    }
}
