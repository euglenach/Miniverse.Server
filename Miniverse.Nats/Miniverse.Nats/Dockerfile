FROM nats:2.10.22-alpine AS base

#FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
#USER $APP_UID
#WORKDIR /app
#
#FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["Miniverse.Nats/Miniverse.Nats.csproj", "Miniverse.Nats/"]
#RUN dotnet restore "Miniverse.Nats/Miniverse.Nats.csproj"
#COPY . .
#WORKDIR "/src/Miniverse.Nats"
#RUN dotnet build "Miniverse.Nats.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "Miniverse.Nats.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Miniverse.Nats.dll"]
