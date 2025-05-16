#!/bin/sh

# Change to the src/ directory
cd src/

# Generate an idempotent migration and save the output to Migrations/migration.sql
dotnet ef migrations script --idempotent -o Migrations/migration.sql

# Copy the generated migration script into the container
docker cp Migrations/migration.sql fitness-check-db:/

# Execute the sql script in the container for database fitnesscheck
docker exec fitness-check-db /bin/sh -c 'mariadb -ugibzapp -ppassword fitnesscheck < /migration.sql'