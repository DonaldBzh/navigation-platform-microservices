FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/NavigationPlatform.Gateway/NavigationPlatform.Gateway.Api/NavigationPlatform.Gateway.Api.csproj", "src/NavigationPlatform.Gateway/NavigationPlatform.Gateway.Api/"]
RUN dotnet restore "src/NavigationPlatform.Gateway/NavigationPlatform.Gateway.Api/NavigationPlatform.Gateway.Api.csproj"
COPY . .
WORKDIR "/src/src/NavigationPlatform.Gateway/NavigationPlatform.Gateway.Api"
RUN dotnet build "NavigationPlatform.Gateway.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NavigationPlatform.Gateway.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NavigationPlatform.Gateway.Api.dll"]