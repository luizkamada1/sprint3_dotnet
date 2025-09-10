using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MottuYardApi.Data;
using MottuYardApi.Models;
using Xunit;

namespace MottuYardApi.Tests;

public class YardTests
{
    private AppDbContext NewCtx()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var ctx = new AppDbContext(options);
        SeedData.Initialize(ctx);
        return ctx;
    }

    [Fact]
    public void Seed_Should_Create_Patios_Zonas_Motos()
    {
        using var ctx = NewCtx();
        Assert.True(ctx.Patios.Count() > 0);
        Assert.True(ctx.Zonas.Count() > 0);
        Assert.True(ctx.Motos.Count() > 0);
    }

    [Fact]
    public async Task Move_Moto_Between_Zones()
    {
        using var ctx = NewCtx();
        var zFrom = ctx.Zonas.First();
        var zTo = ctx.Zonas.Skip(1).First();

        var moto = new Moto { Placa = "TEST1234", Modelo = "CG 160", Status = "Ativa", ZonaId = zFrom.Id };
        ctx.Motos.Add(moto);
        await ctx.SaveChangesAsync();

        moto.ZonaId = zTo.Id;
        await ctx.SaveChangesAsync();

        var loaded = await ctx.Motos.FindAsync(moto.Id);
        Assert.NotNull(loaded);
        Assert.Equal(zTo.Id, loaded!.ZonaId);
    }
}
