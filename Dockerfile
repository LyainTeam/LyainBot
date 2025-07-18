# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY LyainBot/LyainBot.csproj ./LyainBot/
RUN dotnet restore ./LyainBot/LyainBot.csproj

# Copy the rest of the source code
COPY . .

# Build the project
WORKDIR /app/LyainBot
RUN dotnet build "LyainBot.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "LyainBot.csproj" -c Release -o /app/publish /p:PublishSingleFile=true

# Use the official .NET Runtime image for the final stage
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /config

# Copy the published application from the publish stage
COPY --from=publish /app/publish /app

# Set the entry point for the application
RUN chmod +x /app/LyainBot
ENTRYPOINT ["/app/LyainBot"]