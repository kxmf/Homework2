using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Homework2.Api.Converters;
using Homework2.Api.GrpcServices;
using Homework2.Api.Interceptors;
using Homework2.Api.Middlewares;
using Homework2.Api.Validators;
using Homework2.Application;
using Homework2.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services
    .AddGrpc(options =>
    {
        options.Interceptors.Add<LoggingInterceptor>();
        options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    })
    .AddJsonTranscoding();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new ProductTypeJsonConverter());
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Products Service API",
        Version = "v1"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    options.TagActionsBy(api =>
    {
        if (api.GroupName is not null)
            return [api.GroupName];

        if (api.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller))
            return [controller!];

        return ["Other"];
    });

    options.OrderActionsBy(api => $"{api.GroupName ?? api.ActionDescriptor.RouteValues["controller"]}_{api.HttpMethod}");
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Products Service API v1");
    options.RoutePrefix = "swagger";

    options.DisplayRequestDuration();
    options.DefaultModelsExpandDepth(2);
    options.DefaultModelExpandDepth(2);
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.EnableTryItOutByDefault();
});

app.MapControllers();
app.MapGrpcService<ProductGrpcService>();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
