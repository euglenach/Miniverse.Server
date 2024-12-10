FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
EXPOSE 7149

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Miniverse.LogicLooper/Miniverse.LogicLooper.csproj", "Miniverse.LogicLooper/"]
RUN dotnet restore "Miniverse.LogicLooper/Miniverse.LogicLooper.csproj"
COPY . .
WORKDIR "/src/Miniverse.LogicLooper"
RUN dotnet build "Miniverse.LogicLooper.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Miniverse.LogicLooper.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Miniverse.LogicLooper.dll"]
