# Use the ASP.NET Core runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Specify the base image with the SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OTPManager.csproj", "."]
RUN dotnet restore "./OTPManager.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./OTPManager.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy the Oracle Managed ODP.NET package files into the image
COPY ./bin/Debug/net8.0/Oracle.ManagedDataAccess.dll /app
COPY ./bin/Debug/net8.0/Oracle.ManagedDataAccess.xml /app

# Copy the published application files
COPY --from=publish /app/publish .

# Specify the entry point for running the application
ENTRYPOINT ["dotnet", "OTPManager.dll"]
