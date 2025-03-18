# Manager/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Manager/Manager.csproj", "Manager/"]
COPY ["DTO/DTO.csproj", "DTO/"]

RUN dotnet restore "Manager/Manager.csproj"

COPY . .

WORKDIR "/src/Manager"
RUN dotnet build "Manager.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
WORKDIR "/src/Manager"
RUN dotnet publish "Manager.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8082
ENTRYPOINT ["dotnet", "Manager.dll"]