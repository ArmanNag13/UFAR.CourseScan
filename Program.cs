using Microsoft.EntityFrameworkCore;
using UFAR.PDFSync.DAO;
using UFAR.PDFSync.Services;
using UFAR.TimeManagmentTracker.Backend.Services;

namespace UFAR.PDFSync
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Database Context
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<IAIService, AIService>();
            builder.Services.AddScoped<PdfService>(); // Ensure PdfService is registered.
            builder.Services.AddScoped<IPdfService, PdfService>();
            builder.Services.AddScoped<IPdfParser, PdfParser>();
            builder.Services.AddScoped<ICourseParserService,CourseParserService >();
            ;
            builder.Services.AddScoped<ISyllabusService, SyllabusService>();
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
            });





            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }

    }
}
