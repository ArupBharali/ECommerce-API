# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the csproj and restore any dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the entire project and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose the port the app runs on
EXPOSE 5000

# Define the entry point for the container
ENTRYPOINT ["dotnet", "ECommerceApiGateway.dll"]