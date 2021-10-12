using DistributedId.DbContext;
using DistributedId.Helper;
using FreeRedis;
using FreeSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;

namespace DistributedId
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
            //×¢Èë freeredis
            services.AddSingleton(new RedisClient(Configuration.GetConnectionString("freeredis")));
            services.AddScoped<RedisLock>();
            services.AddScoped<RedisCache>();
            //services.AddScoped(typeof(DelayedQueue<>));

            //×¢Èë freesql
            services.AddSingleton(new FreeSqlBuilder().UseConnectionString(FreeSql.DataType.PostgreSQL,
                Configuration.GetConnectionString("freesql")).Build<OrderContext>());
            



            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DistributedId", Version = "v1" });
                c.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
                });

                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                c.IncludeXmlComments(Path.Combine(basePath, "DistributedId.xml"), true);
                c.IgnoreObsoleteActions();
                c.DocInclusionPredicate((docName, description) => true);
            });

            //services.AddDataProtection(x => x.ApplicationDiscriminator = "site.com");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DistributedId v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
