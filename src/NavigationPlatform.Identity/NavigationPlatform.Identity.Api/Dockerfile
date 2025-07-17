# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /App

# Copy everything
COPY . ./
# Restore and publish
RUN dotnet restore src/NavigationPlatform.Identity/NavigationPlatform.Identity.Api/NavigationPlatform.Identity.Api.csproj
RUN dotnet publish src/NavigationPlatform.Identity/NavigationPlatform.Identity.Api/NavigationPlatform.Identity.Api.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /App

COPY --from=build /App/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "NavigationPlatform.Identity.Api.dll"]
