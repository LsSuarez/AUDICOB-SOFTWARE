# Usar la imagen base de .NET 9.0 SDK
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Usar la imagen base de .NET 9.0 SDK para la construcción
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Audicob/Audicob.csproj", "Audicob/"]
RUN dotnet restore "Audicob/Audicob.csproj"
COPY . .
WORKDIR "/src/Audicob"
RUN dotnet build "Audicob.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Audicob.csproj" -c Release -o /app/publish

# Fase final: para ejecutar la aplicación
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Audicob.dll"]
