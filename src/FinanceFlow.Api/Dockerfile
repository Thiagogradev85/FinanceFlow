# ── Stage 1: build ──────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia apenas os csproj primeiro para aproveitar o cache de layer do Docker
# (restore só roda de novo se algum .csproj mudar)
COPY FinanceFlow.slnx ./
COPY src/FinanceFlow.Api/FinanceFlow.Api.csproj                                         src/FinanceFlow.Api/
COPY src/FinanceFlow.SharedKernel/FinanceFlow.SharedKernel.csproj                       src/FinanceFlow.SharedKernel/
COPY src/Modules/Accounts/FinanceFlow.Modules.Accounts.Application/FinanceFlow.Modules.Accounts.Application.csproj         src/Modules/Accounts/FinanceFlow.Modules.Accounts.Application/
COPY src/Modules/Accounts/FinanceFlow.Modules.Accounts.Domain/FinanceFlow.Modules.Accounts.Domain.csproj                   src/Modules/Accounts/FinanceFlow.Modules.Accounts.Domain/
COPY src/Modules/Accounts/FinanceFlow.Modules.Accounts.Infrastructure/FinanceFlow.Modules.Accounts.Infrastructure.csproj   src/Modules/Accounts/FinanceFlow.Modules.Accounts.Infrastructure/
COPY src/Modules/Transactions/FinanceFlow.Modules.Transactions.Application/FinanceFlow.Modules.Transactions.Application.csproj         src/Modules/Transactions/FinanceFlow.Modules.Transactions.Application/
COPY src/Modules/Transactions/FinanceFlow.Modules.Transactions.Domain/FinanceFlow.Modules.Transactions.Domain.csproj                   src/Modules/Transactions/FinanceFlow.Modules.Transactions.Domain/
COPY src/Modules/Transactions/FinanceFlow.Modules.Transactions.Infrastructure/FinanceFlow.Modules.Transactions.Infrastructure.csproj   src/Modules/Transactions/FinanceFlow.Modules.Transactions.Infrastructure/
COPY src/FinanceFlow.Messaging.Kafka/FinanceFlow.Messaging.Kafka.csproj                                                               src/FinanceFlow.Messaging.Kafka/

RUN dotnet restore src/FinanceFlow.Api/FinanceFlow.Api.csproj

# Copia o restante e publica
COPY . .
RUN dotnet publish src/FinanceFlow.Api/FinanceFlow.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: runtime ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Railway injeta a porta via $PORT; o ASP.NET lê ASPNETCORE_URLS automaticamente
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FinanceFlow.Api.dll"]
