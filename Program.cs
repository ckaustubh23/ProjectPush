using Serilog;
using VendorBilling.Application.Common.Utility;
using VendorBilling.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.ConfigureSerilog();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.useDbContext(builder.Configuration);
builder.Services.UseDapperDb();
builder.Services.UseUnitOfWork();

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", builder =>
    {
        builder.WithOrigins("*") 
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
builder.Services.useJWTAuth(builder.Configuration);
builder.Services.useSwaggerOptions();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandler>();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("MyCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
