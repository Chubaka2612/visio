# Stage 1: Base image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

# Stage 2: Build stage using the .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Visio.Web/Visio.Web.csproj", "./Visio.Web/"]
RUN dotnet restore "./Visio.Web/Visio.Web.csproj"

# Copy the rest of the source code and build the application
COPY . .
WORKDIR "/src/Visio.Web"
RUN dotnet build "Visio.Web.csproj" -c Release -o /app/build

# Stage 3: Publish stage to prepare the application for deployment
FROM build AS publish
RUN dotnet publish "Visio.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final image with only runtime and published files
FROM base AS final
WORKDIR /app

# Copy the published application from the previous stage
COPY --from=publish /app/publish .

# Set environment variable to define the URL to listen on
ENV ASPNETCORE_URLS=http://+:5000

# Define the entry point for the container
ENTRYPOINT ["dotnet", "Visio.Web.dll"]
