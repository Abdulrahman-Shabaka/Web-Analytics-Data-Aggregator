FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AnalyticsAggregator.API/AnalyticsAggregator.API.csproj", "src/AnalyticsAggregator.API/"]
COPY ["src/AnalyticsAggregator.Core/AnalyticsAggregator.Core.csproj", "src/AnalyticsAggregator.Core/"]
COPY ["src/AnalyticsAggregator.Infrastructure/AnalyticsAggregator.Infrastructure.csproj", "src/AnalyticsAggregator.Infrastructure/"]
RUN dotnet restore "src/AnalyticsAggregator.API/AnalyticsAggregator.API.csproj"
COPY . .
COPY ["Data", "/src/Data"]
WORKDIR "/src/src/AnalyticsAggregator.API"
RUN dotnet build "AnalyticsAggregator.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AnalyticsAggregator.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src/Data ./Data
COPY --from=build /src/src/AnalyticsAggregator.API/wwwroot ./wwwroot
ENTRYPOINT ["dotnet", "AnalyticsAggregator.API.dll"]

