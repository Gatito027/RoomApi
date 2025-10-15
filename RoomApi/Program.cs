using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using RoomApi;
using RoomApi.Data;

var builder = WebApplication.CreateBuilder(args);

//SERVICIOS REGISTRADOS CORRECTAMENTE
builder.Services.AddSingleton<AppDbContext>();
builder.Services.AddAutoMapper(typeof(MappingConfig));


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