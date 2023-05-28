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

RUN ./gradew clean && ./gradlew build -x test

# Expose the port that the application will run on
EXPOSE 8080

ENTRYPOINT ["java", "-Xms256m", "-Xmx12800m", "-Davailability=98",  "-jar", "build/libs/fakeProvider-0.0.1-SNAPSHOT.jar"]

