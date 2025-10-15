using AutoMapper;
using CloudinaryDotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RoomApi;
using RoomApi.Data;
using RoomApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

//SERVICIOS REGISTRADOS CORRECTAMENTE
builder.Services.AddSingleton<AppDbContext>();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddSingleton(sp =>
{
    var settings = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
    return new Cloudinary(account);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//VERIFICA QUE LOS SERVICIOS ESTÉN REGISTRADOS
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Opcional: Log para verificar servicios
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Servicios registrados correctamente");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();