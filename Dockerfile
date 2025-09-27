# ===== build stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore
COPY ["ATSProject.csproj", "./"]
RUN dotnet restore "./ATSProject.csproj"

# copy everything and publish
COPY . .
RUN dotnet publish "ATSProject.csproj" -c Release -o /app/publish

# ===== runtime stage =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# 🔑 Install required libraries for SQL Server client + EF Core
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6 \
    libicu-dev \
    libkrb5-3 \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

# copy published app
COPY --from=build /app/publish .

# expose container port
EXPOSE 5000

# entrypoint (bind to container's port)
ENTRYPOINT ["sh", "-c", "dotnet ATSProject.dll --urls http://0.0.0.0:${PORT:-5000}"]
