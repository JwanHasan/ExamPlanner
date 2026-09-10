FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /Server

COPY ["ExamPlannerServer/ExamPlannerServer.csproj", "ExamPlannerServer/"]
COPY ["DatabaseConnection/DatabaseConnection.csproj", "DatabaseConnection/"]

RUN dotnet restore "ExamPlannerServer/ExamPlannerServer.csproj"

COPY . .
WORKDIR /Server/ExamPlannerServer
RUN dotnet publish "ExamPlannerServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:latest

WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT [ "dotnet","ExamPlannerServer.dll" ]