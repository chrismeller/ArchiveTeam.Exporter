FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0@sha256:8a90a473da5205a16979de99d2fc20975e922c68304f5c79d564e666dc3982fc AS build
ARG TARGETARCH
WORKDIR /src

COPY src/ArchiveTeam.Exporter.ApiService/ArchiveTeam.Exporter.ApiService.csproj src/ArchiveTeam.Exporter.ApiService/
COPY src/ArchiveTeam.Exporter.ServiceDefaults/ArchiveTeam.Exporter.ServiceDefaults.csproj src/ArchiveTeam.Exporter.ServiceDefaults/
RUN dotnet restore src/ArchiveTeam.Exporter.ApiService/ArchiveTeam.Exporter.ApiService.csproj -a $TARGETARCH

COPY . .
RUN dotnet publish src/ArchiveTeam.Exporter.ApiService/ArchiveTeam.Exporter.ApiService.csproj \
    -a $TARGETARCH \
    --no-restore \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:55e37c7795bfaf6b9cc5d77c155811d9569f529d86e20647704bc1d7dd9741d4 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ArchiveTeam.Exporter.ApiService.dll"]