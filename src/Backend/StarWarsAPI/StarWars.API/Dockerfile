FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["StarWarsAPI/StarWars.API/StarWars.API.csproj", "StarWarsAPI/StarWars.API/"]
COPY ["StarWarsAPI/StarWars.Domain/StarWars.Domain.csproj", "StarWarsAPI/StarWars.Domain/"]
COPY ["StarWarsAPI/StarWars.Infraestructure/StarWars.Infraestructure.csproj", "StarWarsAPI/StarWars.Infraestructure/"]
COPY ["StarWarsAPI/StarWars.Services/StarWars.Services.csproj", "StarWarsAPI/StarWars.Services/"]

RUN dotnet restore "StarWarsAPI/StarWars.API/StarWars.API.csproj"
COPY . .
WORKDIR "/src/StarWarsAPI/StarWars.API"
RUN dotnet build "StarWars.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StarWars.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StarWars.API.dll"]