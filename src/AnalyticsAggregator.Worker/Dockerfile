FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AnalyticsAggregator.Worker/AnalyticsAggregator.Worker.csproj", "src/AnalyticsAggregator.Worker/"]
COPY ["src/AnalyticsAggregator.Core/AnalyticsAggregator.Core.csproj", "src/AnalyticsAggregator.Core/"]
COPY ["src/AnalyticsAggregator.Infrastructure/AnalyticsAggregator.Infrastructure.csproj", "src/AnalyticsAggregator.Infrastructure/"]
RUN dotnet restore "src/AnalyticsAggregator.Worker/AnalyticsAggregator.Worker.csproj"
COPY . .
WORKDIR "/src/src/AnalyticsAggregator.Worker"
RUN dotnet build "AnalyticsAggregator.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AnalyticsAggregator.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AnalyticsAggregator.Worker.dll"]

