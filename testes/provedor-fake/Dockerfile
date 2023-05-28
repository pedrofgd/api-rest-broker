# Use an official OpenJDK runtime as a parent image
FROM openjdk:17

# Install tool for use xargs
RUN microdnf install findutils

# Set the working directory to /app
WORKDIR /app

# Copy the Gradle configuration files to the container
# COPY build.gradle gradlew gradlew.bat settings.gradle .gradle ./

# Copy the source code to the container
# COPY src ./src

# Copiar tudo (para testar...)
COPY . . 

# Expose the port that the application will run on
EXPOSE 8080

ENTRYPOINT ["./gradlew", "bootRun", "--no-daemon", "--args=--server.port=8080", "-PjvmArgs=-Davailability=98"]

