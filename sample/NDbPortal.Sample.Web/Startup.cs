using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NDbPortal.Names;

namespace NDbPortal.Sample.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddNDbPortal(options =>
            {
                options.ConnectionStrings = new ConnectionStrings()
                {
                    DefaultConnectionString =
                        "Server=127.0.0.1;Port=5432;Database=ltv;User Id=postgres;Password = nabin"
                };
                options.DefaultSchema = "ltv_dev";
            });


            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvcWithDefaultRoute();
        }
    }
}
