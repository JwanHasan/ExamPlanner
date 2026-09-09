FROM mcr.microsoft.com/dotnet/sdk:latest as build
WORKDIR /DatabaseConnection

COPY ["DatabaseConnection.csproj",""]
RUN dotnet restore "DatabaseConnection.csproj" 

COPY . .
RUN dotnet publish "DatabaseConnection.csproj" -o Release -c /app/publish p:/UseAppHost=false

FROM mcr.microsoft.com/aspnet/sdk:latest
