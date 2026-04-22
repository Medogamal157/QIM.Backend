FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/QIM.WebApi/QIM.WebApi.csproj", "src/QIM.WebApi/"]
COPY ["src/QIM.Application/QIM.Application.csproj", "src/QIM.Application/"]
COPY ["src/QIM.Domain/QIM.Domain.csproj", "src/QIM.Domain/"]
COPY ["src/QIM.Persistence/QIM.Persistence.csproj", "src/QIM.Persistence/"]
COPY ["src/QIM.Infrastructure/QIM.Infrastructure.csproj", "src/QIM.Infrastructure/"]
COPY ["src/QIM.Presentation/QIM.Presentation.csproj", "src/QIM.Presentation/"]
COPY ["src/QIM.Shared/QIM.Shared.csproj", "src/QIM.Shared/"]
RUN dotnet restore "src/QIM.WebApi/QIM.WebApi.csproj"
COPY . .
WORKDIR "/src/src/QIM.WebApi"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "QIM.WebApi.dll"]
