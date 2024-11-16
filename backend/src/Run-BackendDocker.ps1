# Start by building our dockerfiles
$version = "0.0.1"
$databaseName = "budgetdb"
$dataFiles = "C:\Users\ribul\Documents\budgetdatabasedatafiles"
$postgresUsername = "postgres"
$postgresPassword = "postgres"
$restHttpPort = "14140"
$grpcHttpPort = "14141"

docker build . -t budget-grpc-server:$version

cd .\BudgetDatabase
docker build . -t budget-database-deployer:$version

cd ..

# Then we'll start the docker containers
# Postgres
$postgresContainerId = docker run -d `
	--name budget-database-postgres `
	-e POSTGRES_PASSWORD=$postgresPassword `
	-v $dataFiles`:"/var/lib/postgresql/data" `
    -p 5432:5432 `
	postgres

# Wait for Postgres
do
{
    $out = docker exec budget-database-postgres pg_isready -U $postgresUsername
    $isActive = $out.Split([Environment]::NewLine)[-1]
    $isActive
} While($isActive -notcontains "/var/run/postgresql:5432 - accepting connections")

# Get the ip from the postgres container
$out = docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' $postgresContainerId 
$postgresServername = $out.Split([Environment]::NewLine)[-1]

# DbUp
docker run `
    --name postgres-dbup `
    --rm `
    -e POSTGRES_SERVER=$postgresServername `
    -e POSTGRES_USERNAME=$postgresUsername `
    -e POSTGRES_PASSWORD=$postgresPassword `
    -e POSTGRES_PORT=5432 `
    -e DATABASE_NAME=$databaseName `
    budget-database-deployer:$version

# API
docker run -d `
    --name budget-grpc `
    -e PostgreSqlConnectionDetails__ServerName=$postgresServername `
    -e PostgreSqlConnectionDetails__Username=$postgresUsername `
    -e PostgreSqlConnectionDetails__Password=$postgresPassword `
    -e PostgreSqlConnectionDetails__Port=5432 `
    -e BudgetDatabaseName=$databaseName `
    -e ASPNETCORE_ENVIRONMENT=Development `
    -p $restHttpPort`:5000 `
    -p $grpcHttpPort`:5001 `
    budget-grpc-server:$version 