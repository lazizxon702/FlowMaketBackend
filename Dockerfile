# FlowMarketService API — .NET 9
# Build: docker build -t flowmarket-api .
# Run (PostgreSQL konteyner yoki hostdagi port uchun Host=host.docker.internal yoki servis nomi):
#   docker run --rm -p 8080:8080 \
#     -e ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=...;Username=...;Password=..." \
#     -e Jwt__SigningKey="kamida_32_belgi_uzun_kalit________________" \
#     -e ENABLE_SWAGGER=true \
#     flowmarket-api
#
# Health: GET http://localhost:8080/

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY FlowMarketService/FlowMarketService.csproj FlowMarketService/
RUN dotnet restore FlowMarketService/FlowMarketService.csproj

COPY FlowMarketService/ FlowMarketService/
WORKDIR /src/FlowMarketService
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "FlowMarketService.dll"]
