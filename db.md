# Database Connection Options (Local Development)

Ranked from best to least ideal for this project.

---

## 1. LocalDB (Recommended)

Ships with Visual Studio. Zero configuration, no service to manage, lightweight.

```
Server=(localdb)\MSSQLLocalDB;Database=NmuShuttle;Trusted_Connection=True;TrustServerCertificate=True;
```

**Connect in SSMS:**
- Server name: `(localdb)\MSSQLLocalDB`
- Authentication: Windows Authentication

**Start it:**
```powershell
sqllocaldb start MSSQLLocalDB
```

**Pros:** No install needed (comes with VS), auto-starts on demand, per-user isolation.
**Cons:** Windows only, no remote access, limited to 10 GB per database.

---

## 2. SQL Server Express (Local Instance)

Free edition, runs as a Windows service. Good if you need a persistent always-on server.

```
Server=localhost\SQLEXPRESS;Database=NmuShuttle;Trusted_Connection=True;TrustServerCertificate=True;
```

**Connect in SSMS:**
- Server name: `localhost\SQLEXPRESS` or `.\SQLEXPRESS`
- Authentication: Windows Authentication

**Requires:**
- SQL Server Express installed
- SQL Server Browser service running (for named instances)
- TCP/IP enabled in SQL Server Configuration Manager

**Pros:** Closer to production behavior, supports remote connections, runs as a service.
**Cons:** Requires separate install, uses more resources, 10 GB database size limit.

---

## 3. SQL Server Developer Edition (Full Local Instance)

Full-featured SQL Server, free for development use. Same engine as Enterprise.

```
Server=localhost;Database=NmuShuttle;Trusted_Connection=True;TrustServerCertificate=True;
```

**Connect in SSMS:**
- Server name: `localhost` or `.`
- Authentication: Windows Authentication

**Pros:** Full feature set (partitioning, compression, etc.), no size limits, mirrors production.
**Cons:** Heavier install, uses more RAM/CPU, overkill for most dev work.

---

## 4. SQL Server in Docker Container

Runs SQL Server Linux edition in a container. Good for team consistency.

```powershell
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Pass123" -p 1433:1433 --name sql-dev -d mcr.microsoft.com/mssql/server:2022-latest
```

```
Server=localhost,1433;Database=NmuShuttle;User Id=sa;Password=YourStrong!Pass123;TrustServerCertificate=True;
```

**Connect in SSMS:**
- Server name: `localhost,1433`
- Authentication: SQL Server Authentication
- Login: `sa` / your password

**Pros:** Reproducible, disposable, no Windows service clutter, works cross-platform.
**Cons:** Requires Docker Desktop, uses SA password (not Windows Auth), container management overhead.

---

## 5. Azure SQL Database (Remote)

Cloud-hosted. Only use for shared dev/staging environments, not local iteration.

```
Server=tcp:your-server.database.windows.net,1433;Database=NmuShuttle;User Id=your-user;Password=your-pass;Encrypt=True;TrustServerCertificate=False;
```

**Connect in SSMS:**
- Server name: `your-server.database.windows.net`
- Authentication: SQL Server Authentication (or Azure AD)

**Pros:** No local install, team can share one DB, matches production infra.
**Cons:** Requires internet, costs money, latency on every query, slower dev loop.

---

## Quick Setup (Recommended Path)

1. Open PowerShell and verify LocalDB exists:
   ```powershell
   sqllocaldb info
   ```

2. If `MSSQLLocalDB` is listed, start it:
   ```powershell
   sqllocaldb start MSSQLLocalDB
   ```

3. Set connection string in `Backend/NmuShuttle.Api/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=NmuShuttle;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

4. Run migrations from the `Backend` folder:
   ```powershell
   dotnet ef database update --project NmuShuttle.Api
   ```

5. Connect in SSMS with server name `(localdb)\MSSQLLocalDB` and Windows Authentication.
