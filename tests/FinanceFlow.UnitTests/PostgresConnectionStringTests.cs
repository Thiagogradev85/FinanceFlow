using FinanceFlow.Api.Common;
using Npgsql;

namespace FinanceFlow.UnitTests;

public class PostgresConnectionStringTests
{
    [Fact]
    public void Normalize_UriDoNeon_ConverteParaKeyValue()
    {
        const string uri = "postgresql://neondb_owner:npg_fake_secret@ep-x-pooler.eastus2.azure.neon.tech/neondb?sslmode=require&channel_binding=require";

        var result = PostgresConnectionString.Normalize(uri);

        var parsed = new NpgsqlConnectionStringBuilder(result);
        Assert.Equal("ep-x-pooler.eastus2.azure.neon.tech", parsed.Host);
        Assert.Equal("neondb", parsed.Database);
        Assert.Equal("neondb_owner", parsed.Username);
        Assert.Equal("npg_fake_secret", parsed.Password);
        Assert.Equal(SslMode.Require, parsed.SslMode);
        Assert.Equal(ChannelBinding.Require, parsed.ChannelBinding);
    }

    [Fact]
    public void Normalize_FormatoKeyValue_RetornaIntacto()
    {
        const string keyValue = "Host=localhost;Port=5432;Database=financeflow;Username=financeflow;Password=financeflow123";

        var result = PostgresConnectionString.Normalize(keyValue);

        Assert.Equal(keyValue, result);
    }

    [Fact]
    public void Normalize_UriSemPorta_UsaPortaPadrao()
    {
        const string uri = "postgresql://user:pass@host.neon.tech/db";

        var result = PostgresConnectionString.Normalize(uri);

        var parsed = new NpgsqlConnectionStringBuilder(result);
        Assert.Equal(5432, parsed.Port);
    }

    [Fact]
    public void Normalize_SenhaComCaractereEspecial_PreservaValor()
    {
        // %40 = '@' codificado na URI; o builder tem que decodificar e re-escapar certo.
        const string uri = "postgresql://user:p%40ss%3Bword@host.neon.tech/db?sslmode=require";

        var result = PostgresConnectionString.Normalize(uri);

        var parsed = new NpgsqlConnectionStringBuilder(result);
        Assert.Equal("p@ss;word", parsed.Password);
    }
}
