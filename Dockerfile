FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/MyshowBook/Api/MyShowBook.Api.csproj", "src/MyshowBook/Api/"]
RUN dotnet restore "src/MyshowBook/Api/MyShowBook.Api.csproj"

COPY . .
WORKDIR /src/src/MyshowBook/Api
RUN dotnet publish "MyShowBook.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "MyShowBook.Api.dll"]
