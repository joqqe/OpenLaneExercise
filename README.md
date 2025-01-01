# OpenLaneExercise

## To Install

### Database
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=yourStrong(!)Password" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
connectionstring: "Server=127.0.0.1,1433;Password=yourStrong(!)Password;User Id=SA;Initial Catalog=OpenLane-Dev;"

### Logging
#### Bash
docker run --rm -it \
    -p 18888:18888 -p 4317:18889 \
    -d --name aspire-dashboard \
    -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS="true" \
    mcr.microsoft.com/dotnet/aspire-dashboard:8.1.0

#### PowerShell
docker run --rm -it `
    -p 18888:18888 -p 4317:18889 `
    -d --name aspire-dashboard `
    -e DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS="true" `
    mcr.microsoft.com/dotnet/aspire-dashboard:8.1.0

## Running project locally
### Configuring the project to use SQL Server

1. Ensure your connectionstring in `appsettings.json` point to a local SQL Server instance. Please use User-Secrets!
2. Ensure the tool EF was already installed. You can find some help [here](https://docs.microsoft.com/ef/core/miscellaneous/cli/dotnet)

    ```
    dotnet tool update --global dotnet-ef
    ```

3. Open a command prompt in the solution folder and execute the following commands:

    ```
    dotnet restore
    dotnet tool restore
    dotnet ef database update --context AppDbContext -p ./OpenLane.Api/OpenLane.Api.csproj -s ./OpenLane.Api/OpenLane.Api.csproj
    ```

4. Run the application.

    Note: If you need to create migrations, you can use these commands:

    ```
    -- create migration (from solution folder CLI)
    dotnet ef migrations add <InitialCreate> --context AppDbContext -p ./OpenLane.Api/OpenLane.Api.csproj -s ./OpenLane.Api/OpenLane.Api.csproj -o Infrastructure/Migrations

    -- Revert migration
    dotnet ef database update <20240208081423_InitialCreate> --context AppDbContext -p ./OpenLane.Api/OpenLane.Api.csproj -s ./OpenLane.Api/OpenLane.Api.csproj

    -- Remove migration
    dotnet ef migrations remove --context AppDbContext -p ./OpenLane.Api/OpenLane.Api.csproj -s ./OpenLane.Api/OpenLane.Api.csproj
    ```