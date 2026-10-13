# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/SalesManagement.Domain/SalesManagement.Domain.csproj", "src/SalesManagement.Domain/"]
COPY ["src/SalesManagement.Application/SalesManagement.Application.csproj", "src/SalesManagement.Application/"]
COPY ["src/SalesManagement.Infrastructure/SalesManagement.Infrastructure.csproj", "src/SalesManagement.Infrastructure/"]
COPY ["src/SalesManagement.Api/SalesManagement.Api.csproj", "src/SalesManagement.Api/"]

RUN dotnet restore "src/SalesManagement.Api/SalesManagement.Api.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/SalesManagement.Api"
RUN dotnet build "SalesManagement.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SalesManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SalesManagement.Api.dll"]
