# ACP Local Development Setup

This project uses MariaDB in a Docker container for local development.

## Prerequisites

What is needed:

- .NET 8 SDK should be installed (may update to .NET 10 SDK if needed)
- Visual Studio IDE with the ASP.NET and web development workload
- Docker Desktop for Windows

Docker Desktop can be installed manually from the official [Docker website](https://www.docker.com/products/docker-desktop/)

After installation, start Docker Desktop and verify:

```powershell
docker --version
docker compose version
```

Now View your local development environment credentials. *Remember that you may need to go one level down in the directory structure to find the `compose.yaml` file.*
```powershell
docker compose config
```

## 1. Clone the repository

```powershell
git clone <repository-url>
cd ACP
```

## 2. Create the local environment file

Copy `.env.example` to `.env`:

```powershell
Copy-Item .env.example .env
```

Open `.env` and replace the placeholder passwords with your own local development passwords that you've already retreived.

**Do not commit `.env`.**

## 3. Start MariaDB

From the repository root, where `compose.yaml` is located you will now start your local DB instance, run:

```powershell
docker compose up -d
```

Confirm that the database is running:

```powershell
docker compose ps
```


Review logging:

```powershell
docker compose exec mariadb mariadb -u acp_app -p acp_dev
Enter Password: YOUR_LOCAL_PASSWORD
SELECT VERSION();
SHOW DATABASES;
exit;
```

The `acp-mariadb` container should eventually show as healthy.

To inspect startup logs:

```powershell
docker compose logs mariadb
```

## 4. Configure the ASP.NET connection string

Set the connection string using .NET User Secrets: (time to go back up to root now!)

```powershell
dotnet user-secrets set `
  "ConnectionStrings:DefaultConnection" `
  "Server=localhost;Port=3306;Database=acp_dev;User=acp_app;Password=YOUR_LOCAL_PASSWORD;CharSet=utf8mb4;" `
  --project .\ACP\ACP.csproj
```

The password must match `MARIADB_PASSWORD` in your local `.env` file.

## 5. Restore packages

```powershell
dotnet restore
```

## 6. Apply database migrations

```powershell
dotnet ef database update `
  --project .\ACP\ACP.csproj `
  --startup-project .\ACP\ACP.csproj
```

If the `dotnet ef` command is unavailable, install it:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

## 7. Run the application

Run it through Visual Studio, or use:

```powershell
dotnet run --project .\ACP\ACP.csproj
```

## Common database commands

**Start MariaDB:**

```powershell
docker compose up -d
```

**Stop MariaDB while preserving local data:**

```powershell
docker compose stop
```

**Remove the container while preserving local data:**

```powershell
docker compose down
```

**Completely delete and recreate the local database:**

```powershell
docker compose down -v
docker compose up -d
```

> **Warning:** `docker compose down -v` deletes all local database data.

---

## .gitignore requirements

Make sure your `.gitignore` contains:

```gitignore
.env
.env.local
*.db
*.db-shm
*.db-wal
```

And keep these committed:

```
.env.example
compose.yaml
LOCAL_SETUP.md
Migrations/
```