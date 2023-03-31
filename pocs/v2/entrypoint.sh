#!/bin/bash

# Start the Docker Compose stack in the monitor directory
cd ./monitor
docker-compose up -d

# Start the .NET Core application in the ApiBroker directory with dotnet watch
cd ../ApiBroker/src/ApiBroker.API
dotnet watch run