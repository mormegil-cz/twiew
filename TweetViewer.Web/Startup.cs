using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tweetinvi;
using Tweetinvi.Models;

namespace TweetViewer
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
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.AddSingleton(CreateClient().GetAwaiter().GetResult());
        }

        private async Task<ITwitterClient> CreateClient()
        {
            var consumerKey = Configuration["consumerKey"];
            var consumerSecret = Configuration["consumerSecret"];
            var consumerOnlyCredentials = new ConsumerOnlyCredentials(consumerKey, consumerSecret);
            var appClientWithoutBearer = new TwitterClient(consumerOnlyCredentials);

            var bearerToken = await appClientWithoutBearer.Auth.CreateBearerTokenAsync();
            var appCredentials = new ConsumerOnlyCredentials(consumerKey, consumerSecret, bearerToken);
            return new TwitterClient(appCredentials);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "tweet",
                    pattern: "tweet/{id}",
                    new
                    {
                        controller = "Tweet",
                        action = "Detail"
                    });
                endpoints.MapControllerRoute(
                    name: "tweetRetweets",
                    pattern: "tweet/{id}/retweets",
                    new
                    {
                        controller = "Tweet",
                        action = "Retweets"
                    });
                endpoints.MapControllerRoute(
                    name: "user",
                    pattern: "user/{id}",
                    new
                    {
                        controller = "User",
                        action = "Detail"
                    });
                endpoints.MapControllerRoute(
                    name: "root",
                    pattern: "",
                    new
                    {
                        controller = "Home",
                        action = "Index"
                    });
                endpoints.MapControllerRoute(
                    name: "otherControllers",
                    pattern: "{controller}/{action}/{id?}"
                );
            });
        }
    }
}