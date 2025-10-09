FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/User/User.API/User.API.csproj", "src/Services/User/User.API/"]
COPY ["src/Services/User/User.Services/User.Services.csproj", "src/Services/User/User.Services/"]
COPY ["src/Services/User/User.Repositories/User.Repositories.csproj", "src/Services/User/User.Repositories/"]
COPY ["src/Services/User/User.Domain/User.Domain.csproj", "src/Services/User/User.Domain/"]
COPY ["src/BuildingBlocks/Domain/Domain.csproj", "src/BuildingBlocks/Domain/"]
RUN dotnet restore "src/Services/User/User.API/User.API.csproj"
COPY . .
WORKDIR "/src/src/Services/User/User.API"
RUN dotnet build "./User.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./User.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "User.API.dll"]
