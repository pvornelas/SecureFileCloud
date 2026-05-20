using SecureFileCloud.API.Extensions;
using SecureFileCloud.API.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors(CorsPolicyNames.SecureFileCorsPolicy);

app.UseMiddleware<AuditoriaMiddleware>();

app.MapControllers();

app.Run();