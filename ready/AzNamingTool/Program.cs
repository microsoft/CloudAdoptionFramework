using AzureNamingTool.Attributes;
using BlazorDownloadFile;
using Blazored.Toast;
using Microsoft.OpenApi.Models;
using Blazored.Modal;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;
using AzureNamingTool.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMvcCore().AddApiExplorer();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor().AddHubOptions(x => x.MaximumReceiveMessageSize = 102400000);
builder.Services.AddBlazorDownloadFile();
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();

builder.Services.AddSingleton<StateContainer>();

builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<CustomHeaderSwaggerAttribute>();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v" + Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion,
        Title = "Azure Naming Tool API",
        Description = "An ASP.NET Core Web API for managing the Azure Naming tool configuration. All API requests require the configured API Key (found in the site Admin configuration). You can find more details in the <a href=\"https://github.com/microsoft/CloudAdoptionFramework/blob/master/ready/AzNamingTool/USINGTHEAPI.md\" target=\"_new\">Azure Naming Tool API documentation</a>."
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("http://localhost:44332")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts. 
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AzureNamingToolAPI"));

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

//app.UseAuthentication();
//app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
