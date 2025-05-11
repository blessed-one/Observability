FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Visualisation/Visualisation.csproj", "Visualisation/"]
COPY ["Storage/Storage.csproj", "Storage/"]
COPY ["Realisation/Realisation.csproj", "Realisation/"]
RUN dotnet restore "Visualisation/Visualisation.csproj"
COPY . .
WORKDIR "/src/Visualisation"
RUN dotnet build "Visualisation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Visualisation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Visualisation.dll"]
