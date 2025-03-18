# Worker/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Worker/Worker.csproj", "Worker/"]
COPY ["DTO/DTO.csproj", "DTO/"]

RUN dotnet restore "Worker/Worker.csproj"

COPY . .

WORKDIR "/src/Worker"
RUN dotnet build "Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8081
ENTRYPOINT ["dotnet", "Worker.dll"]
