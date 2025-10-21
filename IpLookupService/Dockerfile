FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["IpLookupService/IpLookupService.csproj", "IpLookupService/"]
COPY ["Shared/Shared.csproj", "Shared/"]

RUN dotnet restore "IpLookupService/IpLookupService.csproj"

COPY . .

WORKDIR "/src/IpLookupService"
RUN dotnet build "IpLookupService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IpLookupService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

RUN mkdir -p /app/Logs

COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "IpLookupService.dll"]
