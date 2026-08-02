# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore
COPY EWOMS_CoreAPI/EWOMS_CoreAPI.csproj EWOMS_CoreAPI/
COPY EWOMS_ClassLibrary/EWOMS_ClassLibrary.csproj EWOMS_ClassLibrary/
COPY EWOMS_Application_CQRS/EWOMS_Application_CQRS.csproj EWOMS_Application_CQRS/
COPY EWOMS_ExternalClassLibrary_DTO/EWOMS_ExternalClassLibrary_DTO.csproj EWOMS_ExternalClassLibrary_DTO/
COPY EWOMS_WPF_Administration/EWOMS_WPF_Administration.csproj EWOMS_WPF_Administration/

RUN dotnet restore EWOMS_CoreAPI/EWOMS_CoreAPI.csproj

# Copy everything and build
COPY . .
RUN dotnet publish EWOMS_CoreAPI/EWOMS_CoreAPI.csproj -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
EXPOSE 7107

ENV ASPNETCORE_URLS=http://+:80;http://+:7107
ENTRYPOINT ["dotnet", "EWOMS_CoreAPI.dll"]
