using OcrApi.Services;
using OcrApi;
using Microsoft.EntityFrameworkCore;
using OcrApi.Data;

// Check if testing mode is enabled (for running the sample test)
var commandArgs = Environment.GetCommandLineArgs();
bool testMode = commandArgs.Length > 1 && commandArgs[1] == "--test";

if (testMode)
{
    // Run the test sample
    TestSample.RunTest();
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "OCR API",
        Version = "v1",
        Description = "API for OCR processing of PDF and image files with financial document data extraction",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "OCR API Support",
            Email = "support@ocrapi.com"
        }
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Register OCR services
builder.Services.AddSingleton<IOcrService, TesseractOcrService>();
builder.Services.AddSingleton<IProcessingService, ProcessingService>();
builder.Services.AddSingleton<ICustomsReceiptParser, CustomsReceiptParser>();

// ลงทะเบียน DbContext สำหรับการเชื่อมต่อกับ SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ลงทะเบียน Database Service
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

// Configure CORS if needed
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OCR API v1");
    c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
});

// Comment out HTTPS redirection to avoid the warning
// app.UseHttpsRedirection();
app.UseCors();

// Add static files support
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
