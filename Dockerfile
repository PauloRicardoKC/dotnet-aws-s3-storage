FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Storage.Api/Storage.Api.csproj", "src/Storage.Api/"]
COPY ["src/Storage.Application/Storage.Application.csproj", "src/Storage.Application/"]
COPY ["src/Storage.Domain/Storage.Domain.csproj", "src/Storage.Domain/"]
COPY ["src/Storage.Infrastructure/Storage.Infrastructure.csproj", "src/Storage.Infrastructure/"]
RUN dotnet restore "src/Storage.Api/Storage.Api.csproj"
COPY . .
RUN dotnet publish "src/Storage.Api/Storage.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Storage.Api.dll"]
