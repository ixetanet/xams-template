# docker run --name xamsdb -p 65001:5432 -e POSTGRES_USER=pgadmin -e POSTGRES_PASSWORD=postgrespw -d postgres

# Migration
# export DB_CONNECTION_STRING="Host=localhost:65001;Database=xamsdb;Username=pgadmin;Password=postgrespw;ApplicationName=XamsProject" && dotnet ef migrations add migration
# Dev - update
# export DB_CONNECTION_STRING="Host=localhost:65001;Database=xamsdb;Username=pgadmin;Password=postgrespw;ApplicationName=XamsProject" && dotnet ef database update

