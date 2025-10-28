FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/User/Exam.API/Exam.API.csproj", "src/Services/User/Exam.API/"]
COPY ["src/Services/User/Exam.Services/Exam.Services.csproj", "src/Services/User/Exam.Services/"]
COPY ["src/Services/User/Exam.Repositories/Exam.Repositories.csproj", "src/Services/User/Exam.Repositories/"]
COPY ["src/Services/User/Exam.Domain/Exam.Domain.csproj", "src/Services/User/Exam.Domain/"]
COPY ["src/BuildingBlocks/Domain/Domain.csproj", "src/BuildingBlocks/Domain/"]
RUN dotnet restore "src/Services/User/Exam.API/Exam.API.csproj"
COPY . .
WORKDIR "/src/src/Services/User/Exam.API"
RUN dotnet build "./Exam.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Exam.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Exam.API.dll"]
