FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Miniverse.MaagicOnion/Miniverse.MaagicOnion.csproj", "Miniverse.MaagicOnion/"]
RUN dotnet restore "Miniverse.MaagicOnion/Miniverse.MaagicOnion.csproj"
COPY . .
WORKDIR "/src/Miniverse.MaagicOnion"
RUN dotnet build "Miniverse.MaagicOnion.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Miniverse.MaagicOnion.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Miniverse.MaagicOnion.dll"]
