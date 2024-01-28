# Start by building our dockerfiles
$version = "0.0.1"
$dataFiles = "C:\Users\ribul\Documents\budgetdatabasedatafiles"
$postgresUsername = "postgres"
$postgresPassword = "postgres"
$grpcHttpPort = "14141"

docker build . -t budget-grpc-server:$version

cd .\BudgetDatabase
docker build . -t budget-database-deployer:$version

cd ..

# Then we'll start the docker containers
# Postgres
docker run -d `
	--name budget-database-postgres `
	-e POSTGRES_PASSWORD=$postgresPassword `
	-v $dataFiles`:"/var/lib/postgresql/data" `
	postgres

# Wait for Postgres
do
{
    $isActive = docker exec budget-database-postgres pg_isready -U $postgresUsername
    $isActive
} While($isActive -ne "/var/run/postgresql:5432 - accepting connections")

# Get the ip from the postgres container
$postgresServername = docker inspect -f `
    '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' `
    budget-database-postgres

# DbUp
docker run `
    --name postgres-dbup `
    --rm `
    -e POSTGRES_SERVER=$postgresServername `
    -e POSTGRES_USERNAME=$postgresUsername `
    -e POSTGRES_PASSWORD=$postgresPassword `
    -e POSTGRES_PORT=5432 `
    budget-database-deployer:$version

# API
docker run -d `
    --name budget-grpc `
    -e PostgreSqlConnectionDetails__ServerName=$postgresServername `
    -e PostgreSqlConnectionDetails__Username=$postgresUsername `
    -e PostgreSqlConnectionDetails__Password=$postgresPassword `
    -e PostgreSqlConnectionDetails__Port=5432 `
    -p $grpcHttpPort`:8080 `
    budget-grpc-server:$version