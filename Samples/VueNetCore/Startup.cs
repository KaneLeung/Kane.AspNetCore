using System.Net;
using System.Threading.Tasks;
using Kane.AspNetCore;
using Kane.Extension.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace VueNetCore
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
            services.AddJwtAuthentication();//加入Jwt服务
            services.AddControllers();
            services.AddSpaStaticFiles(configuration => //生产环境路径
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseRouting();
            app.UseStatusCodePages(async context => //重写401的状态码
            {
                var response = context.HttpContext.Response;
                var message = $"{response.StatusCode} {(HttpStatusCode)response.StatusCode}";
                if (response.StatusCode == 401) 
                {
                    response.ContentType = "application/json";
                    var statusCode = response.StatusCode;
                    response.StatusCode = 200;
                    await response.WriteAsync(new { code = statusCode, message }.ToJson());
                }
                await Task.CompletedTask;
            });
            app.UseAuthentication();//加入Jwt认证
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                if (env.IsDevelopment()) spa.UseVueDevelopmentServer("serve");
            });
        }
    }
}