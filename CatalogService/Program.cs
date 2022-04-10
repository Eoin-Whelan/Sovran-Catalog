using CatalogService.Business;
using CatalogService.Model.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);


//  DEPENDENCY INJECTION
builder.Services.Configure<CatalogDatabaseSettings>(
        builder.Configuration.GetSection("CatalogDatabaseSettings"));

var connString = builder.Configuration.GetSection("CatalogDatabaseSettings:ConnectionString").Value;
var db = builder.Configuration.GetSection("CatalogDatabaseSettings:DatabaseName").Value;
var catalog = builder.Configuration.GetSection("CatalogDatabaseSettings:CatalogsCollectionName").Value;

builder.Services.AddScoped<CatalogDatabaseSettings>(x => new CatalogDatabaseSettings
{
    ConnectionString = connString,
    DatabaseName = db,
    CatalogsCollectionName = catalog
});

//builder.Services.AddScoped<ISovranLogger, SovranLogger>(x => new SovranLogger("CatalogService"
//                                                                                ,connString, 
//                                                                                "tat"));


builder.Services.AddCors(o => o.AddPolicy("Dev", builder =>
{
    builder.WithOrigins("http://localhost.com")
           .AllowAnyMethod()
           .AllowAnyHeader();

}));



builder.Services.AddScoped<ICatalogHandler, CatalogHandler>();
//builder.Services.Configure<CatalogDatabaseSettings>(
//    builder.Configuration.GetSection("CatalogDatabase"));
builder.Services.AddHostedService<StockScribe>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
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
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
