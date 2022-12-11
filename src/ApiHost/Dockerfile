FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ApiHost/ApiHost.csproj", "ApiHost/"]
RUN dotnet restore "ApiHost/ApiHost.csproj"
COPY . .
WORKDIR "/src/ApiHost"
RUN dotnet build "ApiHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiHost.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiHost.dll"]
