using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// Add services to the container.
builder.Services.AddControllersWithViews();


string url = config.GetSection("AppSettings")["OTLService"]; //"http://jaeger:4317";


// Configure metrics
builder.Services.AddOpenTelemetryMetrics(build =>
{
    build.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName));
    build.AddHttpClientInstrumentation();
    build.AddAspNetCoreInstrumentation();
    build.AddMeter(builder.Environment.ApplicationName);
    build.AddOtlpExporter(options => options.Endpoint = new Uri(url));
    build.AddConsoleExporter();

});
// Configure tracing
builder.Services.AddOpenTelemetryTracing(build =>
{
    build.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName));
    build.AddHttpClientInstrumentation();
    build.AddAspNetCoreInstrumentation();
    build.AddSource(builder.Environment.ApplicationName);
    build.AddSqlClientInstrumentation( options => {options.SetDbStatementForText  = true; options.RecordException = true;});       
    build.AddOtlpExporter(options => options.Endpoint = new Uri(url));
});

// Configure logging
builder.Logging.AddOpenTelemetry(build =>
{
    build.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(builder.Environment.ApplicationName));
    build.IncludeFormattedMessage = false;
    build.IncludeScopes = true;
    build.ParseStateValues = true;
    build.AddOtlpExporter(options => options.Endpoint = new Uri(url));
    build.AddConsoleExporter();
});

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
//}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});
app.Run();
