# See https://aka.ms/customizecontainer to learn how to customize your debug container
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Demo.MessageListener/Demo.MessageListener.csproj", "Demo.MessageListener/"]
RUN dotnet restore "Demo.MessageListener/Demo.MessageListener.csproj"
COPY . .
WORKDIR "/src/Demo.MessageListener"
RUN dotnet build "Demo.MessageListener.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Demo.MessageListener.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Demo.MessageListener.dll"]