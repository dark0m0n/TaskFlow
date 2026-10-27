FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/TaskFlow.Domain/TaskFlow.Domain.csproj backend/TaskFlow.Domain/
COPY backend/TaskFlow.Application/TaskFlow.Application.csproj backend/TaskFlow.Application/
COPY backend/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj backend/TaskFlow.Infrastructure/
COPY backend/TaskFlow.API/TaskFlow.API.csproj backend/TaskFlow.API/
RUN dotnet restore backend/TaskFlow.API/TaskFlow.API.csproj

COPY backend/ backend/
RUN dotnet publish backend/TaskFlow.API/TaskFlow.API.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN mkdir -p /app/logs && chown app:app /app/logs
COPY --from=build --chown=app:app /app/publish/ .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]
