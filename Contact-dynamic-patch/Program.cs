using ContactApi.Data;
using ContactApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddControllers()
    .AddNewtonsoftJson()
//    .AddNewtonsoftJson(opts =>
   // opts.SerializerSettings.ContractResolver =
     //   new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver())
        ;


builder.Services.AddDbContext<ContactDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//

builder.Services.AddScoped<IContactService, ContactService>();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);
// Add services to the container.

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Core API");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
