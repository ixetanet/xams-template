#export ASPNETCORE_ENVIRONMENT=$2
dotnet ef migrations add $2
dotnet ef database update --connection $1 # -- --environment $2

# docker run --name xams-project -p 65000:5432 -e POSTGRES_USER=pgadmin -e POSTGRES_PASSWORD=postgrespw -d postgres

# Migration
# export DB_CONNECTION_STRING="Host=localhost:65000;Database=Xams;Username=pgadmin;Password=postgrespw;ApplicationName=Xamsapi" && dotnet ef migrations add migration
# Dev - update
# export DB_CONNECTION_STRING="Host=localhost:65000;Database=Xams;Username=pgadmin;Password=postgrespw;ApplicationName=XamsApi" && dotnet ef database update

