using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Pomelo.AspNetCore.Localization;
using Andoromeda.RushHour.Models;

namespace Andoromeda.RushHour
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfiguration(out var Config);
            services.AddMvc();
            services.AddEntityFrameworkMySql()
                .AddDbContext<RhContext>(x =>
                {
                    x.UseMySql(Config["MySQL"]);
                });

            services.AddTimedJob();

            services.AddPomeloLocalization(x =>
            {
                x.AddCulture(new string[] { "en", "en-US", "en-GB" }, new JsonLocalizedStringStore(Path.Combine("Localization", "en-US.json")));
                x.AddCulture(new string[] { "zh", "zh-CN", "zh-Hans", "zh-Hans-CN", "zh-cn" }, new JsonLocalizedStringStore(Path.Combine("Localization", "zh-CN.json")));
                x.AddCulture(new string[] { "ja", "ja-JP" }, new JsonLocalizedStringStore(Path.Combine("Localization", "ja-JP.json")));
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseFrontendLocalizer("/scripts/localization.js");
            app.UseMvcWithDefaultRoute();
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<RhContext>().Database.EnsureCreated();
                app.UseTimedJob();
            }
        }
    }
}
