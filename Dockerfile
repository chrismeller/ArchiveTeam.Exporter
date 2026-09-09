FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0@sha256:4ea6fe75dd36706bb6d8c3c293d4c4315840f5d76ea28ac97def77e3ec487fa5 AS build
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

FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:1fe86375600b62e6566b465da9553eef0621f13c67f40fe764cd8dbb1dee1497 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ArchiveTeam.Exporter.ApiService.dll"]