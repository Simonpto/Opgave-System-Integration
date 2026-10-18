# Build from the repo root so the shared packages are in the context:
#   docker build -f apps/userprofile-microservice/Dockerfile -t userprofile .
#   docker run -p 8000:8080 userprofile

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
# .NET 8+ images listen on 8080 (ASPNETCORE_HTTP_PORTS), not 80.
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Project files first so `dotnet restore` is cached until a dependency changes.
COPY ["global.json", "./"]
COPY ["apps/userprofile-microservice/src/UserProfile.Presentation/UserProfile.Presentation.csproj", "apps/userprofile-microservice/src/UserProfile.Presentation/"]
COPY ["apps/userprofile-microservice/src/UserProfile.Application/UserProfile.Application.csproj", "apps/userprofile-microservice/src/UserProfile.Application/"]
COPY ["apps/userprofile-microservice/src/UserProfile.Domain/UserProfile.Domain.csproj", "apps/userprofile-microservice/src/UserProfile.Domain/"]
COPY ["apps/userprofile-microservice/src/UserProfile.Infrastructure/UserProfile.Infrastructure.csproj", "apps/userprofile-microservice/src/UserProfile.Infrastructure/"]
COPY ["packages/MessageClient/src/MessageClient.csproj", "packages/MessageClient/src/"]
COPY ["packages/Shared.Contracts/src/Shared.Contracts.csproj", "packages/Shared.Contracts/src/"]
RUN dotnet restore "apps/userprofile-microservice/src/UserProfile.Presentation/UserProfile.Presentation.csproj"
COPY . .
WORKDIR "/src/apps/userprofile-microservice/src/UserProfile.Presentation"
RUN dotnet build "./UserProfile.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserProfile.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserProfile.Presentation.dll"]
