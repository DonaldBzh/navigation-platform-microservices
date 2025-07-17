# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /App

# Copy all source files (from root context!)
COPY . ./

RUN dotnet restore src/NavigationPlatform.UserManagement/NavigationPlatform.UserManagement.Api/NavigationPlatform.UserManagement.Api.csproj
RUN dotnet publish src/NavigationPlatform.UserManagement/NavigationPlatform.UserManagement.Api/NavigationPlatform.UserManagement.Api.csproj -c Release -o out

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /App

COPY --from=build /App/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "NavigationPlatform.UserManagement.Api.dll"]
