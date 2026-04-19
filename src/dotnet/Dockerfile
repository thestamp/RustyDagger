FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files
COPY RustyDagger.slnx .
COPY RustyDagger.Shared/RustyDagger.Shared.csproj RustyDagger.Shared/
COPY RustyDagger.Data/RustyDagger.Data.csproj RustyDagger.Data/
COPY RustyDagger.Server/RustyDagger.Server.csproj RustyDagger.Server/
COPY RustyDagger.Client/RustyDagger.Client.csproj RustyDagger.Client/

# Restore
RUN dotnet restore RustyDagger.slnx

# Copy everything else and publish
COPY . .
RUN dotnet build RustyDagger.slnx -c Release --no-restore
RUN dotnet publish RustyDagger.Server/RustyDagger.Server.csproj -c Release -o /app/publish --no-build

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RustyDagger.Server.dll"]
