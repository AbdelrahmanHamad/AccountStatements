# Account Statements API

A Clean Architecture .NET 10 API that generates customer monthly statements and sends emails asynchronously in the background using Hangfire.

---

## How to Run the Project

### Option A: Using Docker (Recommended)
This is the easiest way to run the API along with background processing and storage.

1. **Start the containers:**
   ```bash
   docker compose up --build -d
   ```
2. **Access the features:**
   * **API Swagger Docs:** `http://localhost:5049/swagger`
   * **Hangfire Background Dashboard:** `http://localhost:5049/hangfire`
3. **Shutdown:**
   ```bash
   docker compose down -v
   ```

### Option B: Running Locally (Without Docker)
1. **Restore dependencies:**
   ```bash
   dotnet restore
   ```
2. **Start the API:**
   ```bash
   dotnet run --project AccountStatements.Api/AccountStatements.Api.csproj
   ```
3. **Access the features:**
   * **API Swagger Docs:** `http://localhost:5049/swagger`
   * **Hangfire Background Dashboard:** `http://localhost:5049/hangfire`

---

## Why are the DB files created in the API layer?

You might notice `account_statements.db` and `hangfire.db` files appearing in the API project directory. Here is why:

* **Startup/Working Directory:** SQLite is file-based and defaults to creating the database file in the directory where the application is executing. Since the `AccountStatements.Api` project is the startup project and the entry point of the application, SQLite creates the files there.
* **Volume Mapping in Docker:** By having the database files in the startup directory, it makes containerization incredibly simple. We mount a single host folder (`./data`) to `/app/data` inside the Docker container and redirect the connection strings there. This ensures both databases persist safely on the host machine across container updates and restarts.
