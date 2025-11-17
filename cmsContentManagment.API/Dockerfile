# Stage 1: Restore & Build
# This stage downloads dependencies and builds the application.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy solution and project files to leverage Docker layer caching.
# This way, 'dotnet restore' is only re-run when project dependencies change.
COPY *.sln .
COPY cmsUserManagment.API/*.csproj ./cmsUserManagment.API/
COPY cmsUserManagment.Application/*.csproj ./cmsUserManagment.Application/
COPY cmsUserManagment.Infrastructure/*.csproj ./cmsUserManagment.Infrastructure/
COPY cmsUserManagment.Domain/*.csproj ./cmsUserManagment.Domain/

# Restore NuGet packages for the entire solution.
RUN dotnet restore "./cmsUserManagment.sln"

# Copy the rest of the source code.
COPY . .

# Publish the application, creating a release-ready output.
# The output is placed in the /app/publish directory.
RUN dotnet publish "./cmsUserManagment.API/cmsUserManagment.API.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Final Image
# This stage creates the final, lightweight production image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published output from the 'build' stage.
COPY --from=build /app/publish .

# Expose port 8080. ASP.NET Core apps in containers default to listening on this port.
# Ensure your application is configured to listen on http://*:8080
EXPOSE 8080

# Set the entry point to run the application.
ENTRYPOINT ["dotnet", "cmsUserManagment.API.dll"]