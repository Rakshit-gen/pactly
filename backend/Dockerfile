FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Pactly.Core/Pactly.Core.csproj src/Pactly.Core/
COPY src/Pactly.Api/Pactly.Api.csproj src/Pactly.Api/
RUN dotnet restore src/Pactly.Api/Pactly.Api.csproj

COPY src/ src/
RUN dotnet publish src/Pactly.Api/Pactly.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Pactly.Api.dll"]
