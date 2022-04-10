#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CatalogService/CatalogService.csproj", "CatalogService/"]
COPY ["CatalogService.Utilities/CatalogService.Utilities.csproj", "CatalogService.Utilities/"]
COPY ["CatalogService.Business/CatalogService.Business.csproj", "CatalogService.Business/"]
COPY ["CatalogService.Model/CatalogService.Model.csproj", "CatalogService.Model/"]
RUN dotnet restore "CatalogService/CatalogService.csproj"
COPY . .
WORKDIR "/src/CatalogService"
RUN dotnet build "CatalogService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CatalogService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatalogService.dll"]