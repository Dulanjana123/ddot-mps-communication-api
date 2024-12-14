# Base image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

ARG ENV
ENV ENV=${ENV}

# Environment variable to bind the app to port 3800
ENV ASPNETCORE_URLS=http://+:3800

USER app
WORKDIR /app
EXPOSE 3800

# Stage for building the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DDOT.MPS.Communication.Api/DDOT.MPS.Communication.Api.csproj", "DDOT.MPS.Communication.Api/"]
COPY ["DDOT.MPS.Communication.Core/DDOT.MPS.Communication.Core.csproj", "DDOT.MPS.Communication.Core/"]
COPY ["DDOT.MPS.Communication.Model/DDOT.MPS.Communication.Model.csproj", "DDOT.MPS.Communication.Model/"]
RUN dotnet restore "./DDOT.MPS.Communication.Api/DDOT.MPS.Communication.Api.csproj"
COPY . .
WORKDIR "/src/DDOT.MPS.Communication.Api"
RUN dotnet build "./DDOT.MPS.Communication.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage for publishing the service project
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DDOT.MPS.Communication.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage for production or running the app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the environment variable
ENV ASPNETCORE_ENVIRONMENT=${ENV}

ENTRYPOINT ["dotnet", "DDOT.MPS.Communication.Api.dll", "--urls", "http://+:3800"]
