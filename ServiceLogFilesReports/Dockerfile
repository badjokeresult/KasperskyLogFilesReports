FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ServiceLogFilesReports/ServiceLogFilesReports.csproj", "ServiceLogFilesReports/"]
RUN dotnet restore "ServiceLogFilesReports/ServiceLogFilesReports.csproj"
COPY . .
WORKDIR "/src/ServiceLogFilesReports"
RUN dotnet build "ServiceLogFilesReports.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServiceLogFilesReports.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServiceLogFilesReports.dll"]
