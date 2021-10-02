#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Hedger.Api/Hedger.Api.csproj", "Hedger.Api/"]
RUN dotnet restore "Hedger.Api/Hedger.Api.csproj"
COPY . .
WORKDIR "/src/Hedger.Api"
RUN dotnet build "Hedger.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hedger.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hedger.Api.dll"]