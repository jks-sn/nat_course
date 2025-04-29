# Manager/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Manager/Manager.csproj", "Manager/"]
RUN dotnet restore "Manager/Manager.csproj"
COPY . .
WORKDIR /src/Manager
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8082
ENTRYPOINT ["dotnet", "Manager.dll"]