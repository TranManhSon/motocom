using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyRobotAPI.Services;
using Microsoft.AspNetCore.Cors;


namespace MyRobotAPI
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<RobotService>();


            // Thêm chính sách CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin()    // Cho phép tất cả domain (hoặc chỉ định origin cụ thể bằng .WithOrigins())
                            .AllowAnyHeader()   // Cho phép tất cả header
                            .AllowAnyMethod();  // Cho phép tất cả phương thức HTTP (GET, POST, PUT, DELETE)
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseRouting();

            // Áp dụng chính sách CORS cụ thể
            app.UseCors("AllowAllOrigins");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
