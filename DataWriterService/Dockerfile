#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DataWriterService/DataWriterService.csproj", "DataWriterService/"]
RUN dotnet restore "DataWriterService/DataWriterService.csproj"
COPY . .
WORKDIR "/src/DataWriterService"
RUN dotnet build "DataWriterService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataWriterService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt-get update
RUN apt-get upgrade -y
RUN apt-get install -y curl
ENTRYPOINT ["dotnet", "DataWriterService.dll"]
