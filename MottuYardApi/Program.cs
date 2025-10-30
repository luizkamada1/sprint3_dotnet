using System;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MottuYardApi.Data;
using MottuYardApi.DTOs;
using MottuYardApi.Models;
using MottuYardApi.Security;
using MottuYardApi.Services;
using MottuYardApi.Swagger;
using MottuYardApi.Utils;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("MottuYardDb"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = $"Chave de API enviada no cabeçalho {ApiKeyConstants.HeaderName}.",
        In = ParameterLocation.Header,
        Name = ApiKeyConstants.HeaderName,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "ApiKey",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddRouting(o => o.LowercaseUrls = true);
builder.Services.AddHealthChecks();
builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection(ApiKeyConstants.ConfigurationSection));
builder.Services.AddSingleton<ApiKeyEndpointFilter>();
builder.Services.AddSingleton<MaintenancePredictionService>();

var app = builder.Build();

// Seed
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData.Initialize(ctx);
}

var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"Mottu Yard API {description.ApiVersion}");
    }
});
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapHealthChecks("/health");

const int MaxPageSize = 100;

var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(1, 0)).Build();
var api = app.MapGroup("/api/v{version:apiVersion}");
api.WithApiVersionSet(apiVersionSet);
api.HasApiVersion(new ApiVersion(1, 0));
api.AddEndpointFilter<ApiKeyEndpointFilter>();

// =============== PÁTIOS ===================

var patios = api.MapGroup("/patios");

patios.MapGet(string.Empty, async (AppDbContext db, HttpContext http, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
{
    (page, pageSize) = Pagination.Normalize(page, pageSize, MaxPageSize);

    var query = db.Patios.AsNoTracking().OrderBy(p => p.Id);
    var total = await query.CountAsync();
    var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
        .Select(p => new PatioDto(p.Id, p.Nome, p.Cidade, p.Estado))
        .ToListAsync();

    var result = PagedResult<PatioDto>.Create(items, page, pageSize, total);
    Hateoas.AddCollectionLinks(result, http, "patios");
    return Results.Ok(result);
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Lista pátios")
.WithDescription("Retorna pátios paginados com links HATEOAS.")
.Produces<PagedResult<PatioDto>>(StatusCodes.Status200OK);

patios.MapGet("/{id:int}", async (int id, AppDbContext db, HttpContext http) =>
{
    var entity = await db.Patios.FindAsync(id);
    if (entity is null) return Results.NotFound();

    var dto = new PatioDto(entity.Id, entity.Nome, entity.Cidade, entity.Estado);
    var res = new Resource<PatioDto>(dto);
    Hateoas.AddSelf(res, http, $"patios/{id}");
    Hateoas.AddLink(res, "zonas", http, $"zonas/patio/{id}", "GET");
    return Results.Ok(res);
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Obtém um pátio")
.WithDescription("Retorna um pátio específico com links.")
.Produces<Resource<PatioDto>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

patios.MapPost(string.Empty, async ([FromBody] PatioCreateDto body, AppDbContext db, HttpContext http) =>
{
    var entity = new Patio { Nome = body.Nome, Cidade = body.Cidade, Estado = body.Estado };
    db.Patios.Add(entity);
    await db.SaveChangesAsync();

    var dto = new PatioDto(entity.Id, entity.Nome, entity.Cidade, entity.Estado);
    var res = new Resource<PatioDto>(dto);
    Hateoas.AddSelf(res, http, $"patios/{entity.Id}");
    return Results.Created(Hateoas.GetVersionedPath(http, $"patios/{entity.Id}"), res);
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Cria pátio")
.WithDescription("Cria um novo pátio. Retorna 201 com Location.")
.Produces<Resource<PatioDto>>(StatusCodes.Status201Created);

patios.MapPut("/{id:int}", async (int id, [FromBody] PatioUpdateDto body, AppDbContext db) =>
{
    var entity = await db.Patios.FindAsync(id);
    if (entity is null) return Results.NotFound();
    entity.Nome = body.Nome;
    entity.Cidade = body.Cidade;
    entity.Estado = body.Estado;
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Atualiza pátio")
.WithDescription("Atualiza um pátio existente.")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

patios.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var entity = await db.Patios.FindAsync(id);
    if (entity is null) return Results.NotFound();
    db.Patios.Remove(entity);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Remove pátio")
.WithDescription("Deleta um pátio existente.")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

// =============== ZONAS ===================

var zonas = api.MapGroup("/zonas");

zonas.MapGet(string.Empty, async (AppDbContext db, HttpContext http, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
{
    (page, pageSize) = Pagination.Normalize(page, pageSize, MaxPageSize);

    var query = db.Zonas.Include(z => z.Patio).AsNoTracking().OrderBy(z => z.Id);
    var total = await query.CountAsync();

    var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
        .Select(z => new ZonaDto(z.Id, z.Nome, z.PatioId, z.Patio!.Nome))
        .ToListAsync();

    var result = PagedResult<ZonaDto>.Create(items, page, pageSize, total);
    Hateoas.AddCollectionLinks(result, http, "zonas");
    return Results.Ok(result);
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Lista zonas")
.WithDescription("Retorna zonas paginadas com pátio associado.")
.Produces<PagedResult<ZonaDto>>(StatusCodes.Status200OK);

zonas.MapGet("/{id:int}", async (int id, AppDbContext db, HttpContext http) =>
{
    var z = await db.Zonas.Include(z => z.Patio).FirstOrDefaultAsync(z => z.Id == id);
    if (z is null) return Results.NotFound();
    var dto = new ZonaDto(z.Id, z.Nome, z.PatioId, z.Patio!.Nome);
    var res = new Resource<ZonaDto>(dto);
    Hateoas.AddSelf(res, http, $"zonas/{id}");
    Hateoas.AddLink(res, "motos", http, $"motos/zona/{id}", "GET");
    Hateoas.AddLink(res, "patio", http, $"patios/{z.PatioId}", "GET");
    return Results.Ok(res);
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Obtém uma zona")
.WithDescription("Retorna uma zona com links para pátio e motos.")
.Produces<Resource<ZonaDto>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

zonas.MapGet("/patio/{patioId:int}", async (int patioId, AppDbContext db, HttpContext http) =>
{
    var exists = await db.Patios.AnyAsync(p => p.Id == patioId);
    if (!exists) return Results.NotFound();
    var zonas = await db.Zonas.Where(z => z.PatioId == patioId).AsNoTracking()
        .Select(z => new ZonaDto(z.Id, z.Nome, z.PatioId, ""))
        .ToListAsync();
    var result = PagedResult<ZonaDto>.Create(zonas, 1, zonas.Count == 0 ? 10 : zonas.Count, zonas.Count);
    Hateoas.AddCollectionLinks(result, http, $"zonas/patio/{patioId}");
    return Results.Ok(result);
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Zonas de um pátio")
.WithDescription("Lista zonas pertencentes a um pátio.")
.Produces<PagedResult<ZonaDto>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

zonas.MapPost(string.Empty, async ([FromBody] ZonaCreateDto body, AppDbContext db, HttpContext http) =>
{
    var patio = await db.Patios.FindAsync(body.PatioId);
    if (patio is null) return Results.BadRequest(new { message = "PatioId inválido" });

    var entity = new Zona { Nome = body.Nome, PatioId = body.PatioId };
    db.Zonas.Add(entity);
    await db.SaveChangesAsync();

    var dto = new ZonaDto(entity.Id, entity.Nome, entity.PatioId, patio.Nome);
    var res = new Resource<ZonaDto>(dto);
    Hateoas.AddSelf(res, http, $"zonas/{entity.Id}");
    return Results.Created(Hateoas.GetVersionedPath(http, $"zonas/{entity.Id}"), res);
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Cria zona")
.WithDescription("Cria uma zona em um pátio.")
.Produces<Resource<ZonaDto>>(StatusCodes.Status201Created);

zonas.MapPut("/{id:int}", async (int id, [FromBody] ZonaUpdateDto body, AppDbContext db) =>
{
    var entity = await db.Zonas.FindAsync(id);
    if (entity is null) return Results.NotFound();
    entity.Nome = body.Nome;
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Atualiza zona")
.WithDescription("Atualiza o nome da zona.")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

zonas.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var entity = await db.Zonas.FindAsync(id);
    if (entity is null) return Results.NotFound();
    db.Zonas.Remove(entity);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Remove zona")
.WithDescription("Remove uma zona existente.")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

// =============== MOTOS ===================

var motos = api.MapGroup("/motos");

motos.MapGet(string.Empty, async (AppDbContext db, HttpContext http, [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
{
    (page, pageSize) = Pagination.Normalize(page, pageSize, MaxPageSize);

    var query = db.Motos.Include(m => m.Zona).ThenInclude(z => z!.Patio).AsNoTracking().OrderBy(m => m.Id);
    var total = await query.CountAsync();
    var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
        .Select(m => MotoDto.FromEntity(m))
        .ToListAsync();

    var result = PagedResult<MotoDto>.Create(items, page, pageSize, total);
    Hateoas.AddCollectionLinks(result, http, "motos");
    return Results.Ok(result);
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Lista motos")
.WithDescription("Retorna motos paginadas com zona e pátio.")
.Produces<PagedResult<MotoDto>>(StatusCodes.Status200OK);

motos.MapGet("/{id:int}", async (int id, AppDbContext db, HttpContext http) =>
{
    var m = await db.Motos.Include(m => m.Zona).ThenInclude(z => z!.Patio).FirstOrDefaultAsync(m => m.Id == id);
    if (m is null) return Results.NotFound();
    var dto = MotoDto.FromEntity(m);
    var res = new Resource<MotoDto>(dto);
    Hateoas.AddSelf(res, http, $"motos/{id}");
    if (m.ZonaId != null) Hateoas.AddLink(res, "zona", http, $"zonas/{m.ZonaId}", "GET");
    return Results.Ok(res);
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Obtém uma moto")
.WithDescription("Retorna uma moto com informações de localização.")
.Produces<Resource<MotoDto>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

motos.MapGet("/zona/{zonaId:int}", async (int zonaId, AppDbContext db, HttpContext http) =>
{
    var exists = await db.Zonas.AnyAsync(z => z.Id == zonaId);
    if (!exists) return Results.NotFound();

    var motos = await db.Motos.Where(m => m.ZonaId == zonaId).AsNoTracking().Include(m => m.Zona).ThenInclude(z => z!.Patio)
        .Select(m => MotoDto.FromEntity(m)).ToListAsync();
    var result = PagedResult<MotoDto>.Create(motos, 1, motos.Count == 0 ? 10 : motos.Count, motos.Count);
    Hateoas.AddCollectionLinks(result, http, $"motos/zona/{zonaId}");
    return Results.Ok(result);
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Motos por zona")
.WithDescription("Lista motos alocadas em uma zona específica.")
.Produces<PagedResult<MotoDto>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

motos.MapPost(string.Empty, async ([FromBody] MotoCreateDto body, AppDbContext db, HttpContext http) =>
{
    if (body.ZonaId is not null)
    {
        var z = await db.Zonas.FindAsync(body.ZonaId.Value);
        if (z is null) return Results.BadRequest(new { message = "ZonaId inválida" });
    }
    var entity = new Moto { Placa = body.Placa, Modelo = body.Modelo, Status = body.Status, ZonaId = body.ZonaId };
    db.Motos.Add(entity);
    await db.SaveChangesAsync();

    var dto = await db.Motos.Include(m => m.Zona).ThenInclude(z => z!.Patio).Where(m => m.Id == entity.Id).Select(m => MotoDto.FromEntity(m)).FirstAsync();
    var res = new Resource<MotoDto>(dto);
    Hateoas.AddSelf(res, http, $"motos/{entity.Id}");
    return Results.Created(Hateoas.GetVersionedPath(http, $"motos/{entity.Id}"), res);
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Cria moto")
.WithDescription("Cadastra uma moto. `ZonaId` é opcional.")
.Produces<Resource<MotoDto>>(StatusCodes.Status201Created);

motos.MapPut("/{id:int}", async (int id, [FromBody] MotoUpdateDto body, AppDbContext db) =>
{
    var m = await db.Motos.FindAsync(id);
    if (m is null) return Results.NotFound();

    if (body.ZonaId is not null)
    {
        var z = await db.Zonas.FindAsync(body.ZonaId.Value);
        if (z is null) return Results.BadRequest(new { message = "ZonaId inválida" });
    }

    m.Placa = body.Placa;
    m.Modelo = body.Modelo;
    m.Status = body.Status;
    m.ZonaId = body.ZonaId;
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Atualiza moto")
.WithDescription("Atualiza dados e localização da moto.")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

motos.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var m = await db.Motos.FindAsync(id);
    if (m is null) return Results.NotFound();
    db.Motos.Remove(m);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Remove moto")
.WithDescription("Deleta uma moto cadastrada.")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

// Ação de negócio: mover uma moto entre zonas
motos.MapPost("/{id:int}/mover", async (int id, [FromBody] MotoMoverDto body, AppDbContext db) =>
{
    var m = await db.Motos.FindAsync(id);
    if (m is null) return Results.NotFound();

    var z = await db.Zonas.FindAsync(body.NovaZonaId);
    if (z is null) return Results.BadRequest(new { message = "NovaZonaId inválida" });

    m.ZonaId = body.NovaZonaId;
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Move moto de zona")
.WithDescription("Ação de negócio para realocar moto entre zonas.")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

// =============== ANALYTICS (ML.NET) ===================

var analytics = api.MapGroup("/analytics");

analytics.MapPost("/maintenance-prediction", ([FromBody] MaintenancePredictionRequest body, MaintenancePredictionService service) =>
{
    var prediction = service.Predict(new MaintenanceData
    {
        DaysSinceMaintenance = body.DaysSinceMaintenance,
        CompletedDeliveries = body.CompletedDeliveries,
        BreakdownHistory = body.BreakdownHistory
    });

    var response = new MaintenancePredictionResponse(prediction.RequiresMaintenance, prediction.Probability);
    return Results.Ok(response);
})
.MapToApiVersion(new ApiVersion(1, 0))
.WithSummary("Predição de manutenção de motos")
.WithDescription("Utiliza ML.NET para estimar a necessidade de manutenção preventiva de uma moto.")
.Produces<MaintenancePredictionResponse>(StatusCodes.Status200OK);

// Run
app.Run();

public partial class Program;

namespace MottuYardApi.Utils
{
    public static class Pagination
    {
        public static (int, int) Normalize(int page, int pageSize, int max)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, max);
            return (page, pageSize);
        }
    }

    public static class Hateoas
    {
        public static void AddCollectionLinks<T>(PagedResult<T> page, HttpContext http, string path)
        {
            var links = new List<Link>
            {
                new Link(GetUri(http, path, page.Page, page.PageSize), "self", "GET")
            };
            if (page.Page > 1)
                links.Add(new Link(GetUri(http, path, page.Page - 1, page.PageSize), "prev", "GET"));
            var totalPages = (int)Math.Ceiling((double)page.TotalItems / page.PageSize);
            if (page.Page < totalPages)
                links.Add(new Link(GetUri(http, path, page.Page + 1, page.PageSize), "next", "GET"));

            page.Links = links;
        }

        public static void AddSelf<T>(Resource<T> res, HttpContext http, string path)
        {
            res.Links.Add(new Link(GetBase(http) + GetVersionedPath(http, path), "self", "GET"));
        }

        public static void AddLink<T>(Resource<T> res, string rel, HttpContext http, string path, string method)
        {
            res.Links.Add(new Link(GetBase(http) + GetVersionedPath(http, path), rel, method));
        }

        private static string GetUri(HttpContext http, string path, int page, int pageSize)
        {
            var versionedPath = GetVersionedPath(http, path);
            return $"{GetBase(http)}{versionedPath}?page={page}&pageSize={pageSize}";
        }

        private static string GetBase(HttpContext http)
            => $"{http.Request.Scheme}://{http.Request.Host}";

        public static string GetVersionedPath(HttpContext http, string path)
        {
            path = path.TrimStart('/');
            var version = http.GetRequestedApiVersion();
            var versionSegment = version is null
                ? "v1"
                : version.MinorVersion.GetValueOrDefault() > 0
                    ? $"v{version.MajorVersion}.{version.MinorVersion}"
                    : $"v{version.MajorVersion}";

            if (path.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
            {
                path = path[4..];
            }

            return $"/api/{versionSegment}/{path}";
        }
    }
}

namespace MottuYardApi.DTOs
{
    /// <summary>Link HATEOAS.</summary>
    public record Link(string Href, string Rel, string Method);

    /// <summary>Envelope HATEOAS de um recurso.</summary>
    public class Resource<T>
    {
        public T Data { get; set; }
        public List<Link> Links { get; set; } = new();
        public Resource(T data) => Data = data;
    }

    /// <summary>Resultado paginado.</summary>
    public class PagedResult<T>
    {
        public required List<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public List<Link> Links { get; set; } = new();

        public static PagedResult<T> Create(List<T> items, int page, int pageSize, int total)
            => new() { Items = items, Page = page, PageSize = pageSize, TotalItems = total };
    }
}