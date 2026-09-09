FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /ExamPlannerServer

COPY ["ExamPlannerServer.csproj",""]
RUN dotnet restore "ExamPlannerServer.csproj"

COPY . .
RUN dotnet publish "ExamPlannerServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:latest

WORKDIR /ExamPlannerServer
COPY --from=build /app/publish .

ENTRYPOINT [ "dotnet","ExamPlannerServer.dll" ]