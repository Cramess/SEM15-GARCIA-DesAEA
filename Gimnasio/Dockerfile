FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish Gimnasio.Api/Gimnasio.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# Render inyecta PORT; el proxy entrega X-Forwarded-Proto (evita bucle de HTTPS redirect).
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet Gimnasio.Api.dll"]