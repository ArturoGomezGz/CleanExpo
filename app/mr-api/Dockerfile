FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/MrApi/MrApi.csproj src/MrApi/
RUN dotnet restore src/MrApi/MrApi.csproj
COPY src/MrApi src/MrApi
RUN dotnet publish src/MrApi/MrApi.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "MrApi.dll"]
