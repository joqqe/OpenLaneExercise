# OpenLaneExercise


## To Install
### Database (SQL Server)
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=yourStrong(!)Password" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
connectionstring: "Server=127.0.0.1,1433;Password=yourStrong(!)Password;User Id=SA;Initial Catalog=OpenLane-Dev;"

### Message Queue (RabbitMq)
docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:4.0-management

### Logging (Aspire Dashboard)
docker run --rm -it -p 18888:18888 -p 4317:18889 -d --name aspire-dashboard -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS="true" mcr.microsoft.com/dotnet/aspire-dashboard:8.1.0

## Running Project Locally
### Database
1. Ensure your connectionstring in `appsettings.json` point to a local SQL Server instance.
2. Ensure the tool EF was already installed. You can find some help [here](https://docs.microsoft.com/ef/core/miscellaneous/cli/dotnet)

    ```
    dotnet tool update --global dotnet-ef
    ```

3. Open a command prompt in the solution folder and execute the following commands:

    ```
    dotnet restore
    dotnet tool restore
    dotnet ef database update --context AppDbContext -p ./src/OpenLane.Api/OpenLane.Api.csproj -s ./src/OpenLane.Api/OpenLane.Api.csproj
    ```

4. Run the application.

    Note: If you need to create migrations, you can use these commands:

    ```
    -- create migration (from solution folder CLI)
    dotnet ef migrations add <InitialCreate> --context AppDbContext -p ./src/OpenLane.Api/OpenLane.Api.csproj -s ./src/OpenLane.Api/OpenLane.Api.csproj -o Infrastructure/Migrations

    -- Revert migration
    dotnet ef database update <20240208081423_InitialCreate> --context AppDbContext -p ./src/OpenLane.Api/OpenLane.Api.csproj -s ./src/OpenLane.Api/OpenLane.Api.csproj

    -- Remove migration
    dotnet ef migrations remove --context AppDbContext -p ./src/OpenLane.Api/OpenLane.Api.csproj -s ./src/OpenLane.Api/OpenLane.Api.csproj
    ```

### Api
1. Restore the solution.
    ```
    dotnet restore
    ```
2. Build the solution.
    ```
    dotnet build
    ```
3. Set startup-projects: Api.
4. Run the solution.
    ```
    dotnet run
    ```

## Todos
- Adding cancellationTokens
- Split up Api into Api and Service to consumer messages (better for scaling)
- Adding security
- Add missing endpoints
- Outbox pattern 