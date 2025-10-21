# Use the official .NET 9.0 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["BatchProcessorService/BatchProcessorService.csproj", "BatchProcessorService/"]
COPY ["Shared/Shared.csproj", "Shared/"]

RUN dotnet restore "BatchProcessorService/BatchProcessorService.csproj"

COPY . .

WORKDIR "/src/BatchProcessorService"
RUN dotnet build "BatchProcessorService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BatchProcessorService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

RUN mkdir -p /app/Logs

COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "BatchProcessorService.dll"]
