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

# Copy everything else
COPY . .

# 1. Publish Blazor WASM client standalone (avoids Blazor publish pipeline bug when
#    the client is a project reference of the server in preview SDK builds)
RUN dotnet publish RustyDagger.Client/RustyDagger.Client.csproj -c Release -o /app/client-publish

# 2. Publish server without the client project reference
RUN dotnet publish RustyDagger.Server/RustyDagger.Server.csproj -c Release -o /app/publish \
    -p:ExcludeClientReference=true

# 3. Overlay client wwwroot into server wwwroot so UseBlazorFrameworkFiles() can serve it
RUN mkdir -p /app/publish/wwwroot \
 && cp -r /app/client-publish/wwwroot/. /app/publish/wwwroot/

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RustyDagger.Server.dll"]
