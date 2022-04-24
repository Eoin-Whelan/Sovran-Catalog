using CatalogService.Business;
using CatalogService.Model.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Configuration;
using Sovran.Logger;
using CatalogService.Utilities.Cloudinary;
using CloudinaryDotNet;

var builder = WebApplication.CreateBuilder(args);


//  DEPENDENCY INJECTION
builder.Services.Configure<CatalogDatabaseSettings>(
        builder.Configuration.GetSection("CatalogDatabaseSettings"));

var connString = builder.Configuration.GetSection("CatalogDatabaseSettings:ConnectionString").Value;
var db = builder.Configuration.GetSection("CatalogDatabaseSettings:DatabaseName").Value;
var catalog = builder.Configuration.GetSection("CatalogDatabaseSettings:CatalogsCollectionName").Value;

//  Catalog handler setup
builder.Services.AddScoped<CatalogDatabaseSettings>(x => new CatalogDatabaseSettings
{
    ConnectionString = connString,
    DatabaseName = db,
    CatalogsCollectionName = catalog
});
builder.Services.AddScoped<ICatalogHandler, CatalogHandler>();

//  Image handler setup
builder.Services.AddScoped<Account>(x => new Account
{
    ApiKey = builder.Configuration.GetSection("cloudinaryApiKey").Value,
    ApiSecret = builder.Configuration.GetSection("cloudinaryApiSecret").Value,
    Cloud = builder.Configuration.GetSection("cloudinaryDomain").Value
});
builder.Services.AddScoped<IImageHandler, ImageHandler>();

//  Logger setup
builder.Services.AddScoped<ISovranLogger, SovranLogger>(x => new SovranLogger(
    "Catalog",
    builder.Configuration.GetConnectionString("loggerMongo"),
    builder.Configuration.GetConnectionString("loggerSql")
    )
);
//  Add StockScribe service task.
builder.Services.AddHostedService<StockScribe>(
            x => new StockScribe(builder.Configuration.GetConnectionString("rabbitMq"),
                                    builder.Configuration.GetSection("CatalogDatabaseSettings:ConnectionString").Value));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//  Add SwaggerGen
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Catalog Service API",
        Description =
            "<h2>CatalogController is an API for all Catalog service business logic.</h2><br>" +
            "This functionality extends to:<ul>" +
            "<li> Inserting a new catalog(Registration flow)" +
            "<li> Inserting a new item for a given merchant." +
            "<li> Update an existing item's details." +
            "<li> Updating a merchant's public details (e.g.Support information)" +
            "<li> Retrieving a given merchant's entire catalog (Browsing purposes).</ul>"
        ,
        Contact = new OpenApiContact
        {
            Name = "Eoin Whelan (Farrell)",
            Email = "C00164354@itcarlow.ie",
        }
        
    });
    // Set the comments path for the Swagger JSON and UI.
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

    app.UseCors();

}
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
