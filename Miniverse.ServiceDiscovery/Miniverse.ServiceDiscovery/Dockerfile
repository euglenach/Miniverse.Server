FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Miniverse.ServiceDiscovery/Miniverse.ServiceDiscovery.csproj", "Miniverse.ServiceDiscovery/"]
RUN dotnet restore "Miniverse.ServiceDiscovery/Miniverse.ServiceDiscovery.csproj"
COPY . .
WORKDIR "/src/Miniverse.ServiceDiscovery"
RUN dotnet build "Miniverse.ServiceDiscovery.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Miniverse.ServiceDiscovery.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Miniverse.ServiceDiscovery.dll"]
