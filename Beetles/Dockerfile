FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY Beetles.slnx .
COPY Directory.Packages.props .
COPY src/ src/

RUN dotnet publish src/Beetles.Api/Beetles.Api.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /out .

ENTRYPOINT ["dotnet", "Beetles.Api.dll"]
