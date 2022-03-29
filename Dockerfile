#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

#Added for internal nuget soure fix
RUN apt-get update
RUN apt-get -y install gss-ntlmssp

WORKDIR /src

COPY ["nuget.config", "."]

COPY ["RabbitMQWorker.csproj", "."]
RUN dotnet restore "./RabbitMQWorker.csproj" --configfile "nuget.config"

COPY . .
WORKDIR "/src/."
RUN dotnet build "RabbitMQWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RabbitMQWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitMQWorker.dll"]

EXPOSE 15672
EXPOSE 15671
EXPOSE 5672