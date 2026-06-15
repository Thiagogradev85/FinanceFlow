using Npgsql;

namespace FinanceFlow.Api.Common;

/// <summary>
/// Normaliza a connection string do Postgres aceitando os DOIS formatos:
/// <list type="bullet">
///   <item>URI — <c>postgresql://user:pass@host/db?sslmode=require</c> — é o que provedores
///   de nuvem (Neon, Render, Heroku, Supabase) entregam por padrão.</item>
///   <item>key-value — <c>Host=...;Database=...;Username=...;Password=...</c> — o ÚNICO que o
///   Npgsql (driver .NET) entende nativamente; é o que usamos no dev local.</item>
/// </list>
/// Se a string já vier em key-value, devolve sem tocar. Assim dá pra colar a string do Neon
/// "como ela vem" na env var <c>ConnectionStrings__Postgres</c>, sem tradução manual.
/// </summary>
public static class PostgresConnectionString
{
    public static string Normalize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw;

        var isUri = raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            || raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);

        if (!isUri)
            return raw; // já está em key-value (dev local) — nada a fazer

        var uri = new Uri(raw);
        var userInfo = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port == -1 ? 5432 : uri.Port, // URI sem porta → default do Postgres
            Database = uri.AbsolutePath.Trim('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
        };

        // Query params (?sslmode=require&channel_binding=require) → propriedades do Npgsql.
        foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(kv[0]).ToLowerInvariant();
            var value = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : string.Empty;

            switch (key)
            {
                case "sslmode":
                    builder.SslMode = Enum.Parse<SslMode>(value, ignoreCase: true);
                    break;
                case "channel_binding":
                    builder.ChannelBinding = Enum.Parse<ChannelBinding>(value, ignoreCase: true);
                    break;
            }
        }

        return builder.ConnectionString;
    }
}
