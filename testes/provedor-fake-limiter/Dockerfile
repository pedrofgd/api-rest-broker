FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App
EXPOSE 80
EXPOSE 443

COPY ProvedorFake.Limiter.API .
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "ProvedorFake.Limiter.API.dll"]