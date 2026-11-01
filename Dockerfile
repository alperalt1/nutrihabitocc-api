FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Nutrihabitocc/Nutrihabitocc.csproj Nutrihabitocc/
RUN dotnet restore Nutrihabitocc/Nutrihabitocc.csproj

COPY . .
RUN dotnet publish Nutrihabitocc/Nutrihabitocc.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8421 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build --chown=app:app /app/publish .

USER app

EXPOSE 8421

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl -fsS http://127.0.0.1:8421/health || exit 1

ENTRYPOINT ["dotnet", "Nutrihabitocc.dll"]
