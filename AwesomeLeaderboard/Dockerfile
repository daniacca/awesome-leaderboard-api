#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AwesomeLeaderboard/AwesomeLeaderboard.csproj", "AwesomeLeaderboard/"]
RUN dotnet restore "AwesomeLeaderboard/AwesomeLeaderboard.csproj"
COPY . .
WORKDIR "/src/AwesomeLeaderboard"
RUN dotnet build "AwesomeLeaderboard.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AwesomeLeaderboard.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt-get update
RUN apt-get upgrade -y
RUN apt-get install -y curl
ENTRYPOINT ["dotnet", "AwesomeLeaderboard.dll"]
