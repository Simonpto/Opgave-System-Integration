FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/UserProfile.Presentation/UserProfile.Presentation.csproj", "src/UserProfile.Presentation/"]
COPY ["src/UserProfile.Application/UserProfile.Application.csproj", "src/UserProfile.Application/"]
COPY ["src/UserProfile.Domain/UserProfile.Domain.csproj", "src/UserProfile.Domain/"]
COPY ["src/UserProfile.Infrastructure/UserProfile.Infrastructure.csproj", "src/UserProfile.Infrastructure/"]
COPY ["packages/Shared.Contracts/src/Shared.Contracts.csproj", "packages/Shared.Contracts/src/"]
COPY ["packages/MessageClient/src/MessageClient.csproj", "packages/MessageClient/src/"]
RUN dotnet restore "src/UserProfile.Presentation/UserProfile.Presentation.csproj"
COPY . .
WORKDIR "/src/src/UserProfile.Presentation"
RUN dotnet build "./UserProfile.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserProfile.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserProfile.Presentation.dll"]
