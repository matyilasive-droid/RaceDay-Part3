FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/RaceDay.Api/RaceDay.Api.csproj src/RaceDay.Api/
RUN dotnet restore src/RaceDay.Api/RaceDay.Api.csproj
COPY src/RaceDay.Api/ src/RaceDay.Api/
RUN dotnet publish src/RaceDay.Api/RaceDay.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8081 ASPNETCORE_HTTP_PORTS=8081
EXPOSE 8081
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.Api.dll"]
