# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY apps/api/RestaurantSaas.Api.csproj apps/api/
RUN dotnet restore apps/api/RestaurantSaas.Api.csproj

COPY apps/api/ apps/api/
WORKDIR /src/apps/api
RUN dotnet publish RestaurantSaas.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render injects $PORT at runtime; Program.cs reads it via Environment.GetEnvironmentVariable("PORT")
EXPOSE 10000
ENTRYPOINT ["dotnet", "RestaurantSaas.Api.dll"]
