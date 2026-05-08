using System.Text.Json.Serialization;
using MediatR;
using PropostaService.Application;
using PropostaService.Domain;
using PropostaService.Infrastructure;
using BaseComum;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CriarPropostaCommand).Assembly));
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddPropostaInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

await app.EnsurePropostaDatabaseAsync();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (DomainException exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { erro = exception.Message });
    }
});

app.MapGet("/health", async (PropostaDbContext dbContext, CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
    return canConnect
        ? Results.Ok(new { status = "Healthy", service = "PropostaService", database = "Connected" })
        : Results.Problem("Banco de dados indisponivel.");
});

var propostas = app.MapGroup("/api/propostas");

propostas.MapPost("/", async (CriarPropostaRequest request, IMediator mediator, CancellationToken cancellationToken) =>
{
    var response = await mediator.Send(new CriarPropostaCommand(
        request.NomeCliente,
        request.DocumentoCliente,
        request.TipoSeguro,
        request.ValorSeguro), cancellationToken);
    return Results.Created($"/api/propostas/{response.Id}", response);
});

propostas.MapGet("/", async (PropostaStatus? status, IMediator mediator, CancellationToken cancellationToken) =>
{
    var response = await mediator.Send(new ListarPropostasQuery(status), cancellationToken);
    return Results.Ok(response);
});

propostas.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
{
    var response = await mediator.Send(new BuscarPropostaPorIdQuery(id), cancellationToken);
    return response is null ? Results.NotFound() : Results.Ok(response);
});

propostas.MapPatch("/{id:guid}/status", async (Guid id, AlterarStatusPropostaRequest request, IMediator mediator, CancellationToken cancellationToken) =>
{
    var response = await mediator.Send(new AlterarStatusPropostaCommand(id, request.Status), cancellationToken);
    return response is null ? Results.NotFound() : Results.Ok(response);
});

app.MapGet("/api/tipos-seguro", async (IMediator mediator, CancellationToken cancellationToken) =>
{
    var response = await mediator.Send(new ListarTiposSeguroQuery(), cancellationToken);
    return Results.Ok(response);
});

app.Run();
