FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
EXPOSE 5209

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Miniverse.Server/Miniverse.MagicOnion/Miniverse.MagicOnion/Miniverse.MagicOnion.csproj", "Miniverse.MagicOnion/"]
RUN dotnet restore "Miniverse.MagicOnion/Miniverse.MagicOnion.csproj"
COPY . .
WORKDIR "/src/Miniverse.MagicOnion"
RUN dotnet build "Miniverse.MagicOnion.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Miniverse.MagicOnion.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Miniverse.MagicOnion.dll"]

#COPY ["Miniverse.Shared/MiniverseShared/MiniverseShared/MiniverseShared.csproj", "MiniverseShared/"]
#COPY ["Miniverse.Server/Miniverse.MagicOnion/Miniverse.MagicOnion/Miniverse.MagicOnion.csproj", "Miniverse.Server/Miniverse.MagicOnion/Miniverse.MagicOnion/"]
#RUN dotnet restore "Miniverse.Server/Miniverse.MagicOnion/Miniverse.MagicOnion/Miniverse.MagicOnion.csproj"
#COPY . .
#WORKDIR "/src/Miniverse.Server/Miniverse.MagicOnion/Miniverse.MagicOnion"
#RUN dotnet build "Miniverse.MagicOnion.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "Miniverse.MagicOnion.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Miniverse.Server/Miniverse.MagicOnion/Miniverse.MagicOnion.dll"]