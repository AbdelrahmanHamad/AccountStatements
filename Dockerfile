FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AccountStatements.Domain/AccountStatements.Domain.csproj", "AccountStatements.Domain/"]
COPY ["AccountStatements.Application/AccountStatements.Application.csproj", "AccountStatements.Application/"]
COPY ["AccountStatements.Infrastructure/AccountStatements.Infrastructure.csproj", "AccountStatements.Infrastructure/"]
COPY ["AccountStatements.Api/AccountStatements.Api.csproj", "AccountStatements.Api/"]

RUN dotnet restore "AccountStatements.Api/AccountStatements.Api.csproj"

COPY . .

WORKDIR "/src/AccountStatements.Api"
RUN dotnet publish "AccountStatements.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN mkdir -p /app/data
VOLUME /app/data

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "AccountStatements.Api.dll"]
