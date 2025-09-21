# Build stage for Vue frontend
FROM node:20-alpine AS frontend-build
WORKDIR /app/frontend
COPY DevQuiz.WebClient/package*.json ./
RUN npm ci
COPY DevQuiz.WebClient/ ./
RUN npm run build

# Build stage for .NET backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /app
COPY DevQuiz.API/*.csproj ./DevQuiz.API/
RUN dotnet restore ./DevQuiz.API/DevQuiz.API.csproj
COPY DevQuiz.API/ ./DevQuiz.API/
WORKDIR /app/DevQuiz.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=backend-build /app/publish .
COPY --from=frontend-build /app/frontend/dist ./wwwroot

# Expose port 8080 for the application
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DevQuiz.API.dll"]