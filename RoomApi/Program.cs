using AutoMapper;
using Azure;
using Azure.AI.ContentSafety;
using CloudinaryDotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RoomApi;
using RoomApi.Data;
using RoomApi.Extensions;
using RoomApi.Services;
using RoomApi.Utils;

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
builder.Services.AddSingleton(sp =>
{
    var config = builder.Configuration.GetSection("AzureContentSafety");
    var endpoint = new Uri(config["Endpoint"]);
    var credential = new AzureKeyCredential(config["ApiKey"]);
    return new ContentSafetyClient(endpoint, credential);
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Mis servicios
builder.Services.AddScoped<RoomGetService>();
builder.Services.AddScoped<RoomGetByIdService>();
builder.Services.AddScoped<RoomCreateService>();
builder.Services.AddScoped<RoomDeleteService>();
builder.Services.AddScoped<RoomDeleteByUserService>();
//Utils de terceros
builder.Services.AddScoped<ContentSafetyValidator>();

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
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API registro de cuartos v1");
    c.RoutePrefix = string.Empty;
}
);
app.UseSwagger();

app.Run();