# ── Stage 0: build do frontend (Node) ───────────────────────────────────
# Builda o React/Vite e gera os estáticos em /app/frontend/dist.
FROM node:22-alpine AS frontend
WORKDIR /app/frontend

# Copia manifests primeiro p/ cache de layer (npm ci só re-roda se eles mudarem)
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./
# A API_KEY (env var do Render) chega como build-arg e vira VITE_API_KEY, embutida
# no bundle (é o X-Api-Key que o navegador manda). VITE_API_URL fica VAZIO de
# propósito → o front chama "/api" (mesma origem da própria API que o serve).
ARG API_KEY=""
ENV VITE_API_KEY=$API_KEY
RUN npm run build

# ── Stage 1: build do backend ───────────────────────────────────────────
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

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
# Estáticos do React vão pro wwwroot → a API serve a UI (UseStaticFiles).
COPY --from=frontend /app/frontend/dist ./wwwroot

# Render/Railway injetam a porta via $PORT em RUNTIME. Por isso usamos a forma shell
# do ENTRYPOINT e --urls: a variável só é resolvida quando o container sobe (não no build).
# Se $PORT não existir (Docker local), cai em 8080.
ENTRYPOINT ["sh", "-c", "dotnet FinanceFlow.Api.dll --urls http://+:${PORT:-8080}"]
