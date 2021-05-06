FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Tindo.UgitServer/Tindo.UgitServer.csproj", "Tindo.UgitServer/"]
RUN dotnet restore "src/Tindo.UgitServer/Tindo.UgitServer.csproj"
COPY . .
WORKDIR "/src/Tindo.UgitServer"
RUN dotnet build "Tindo.UgitServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Tindo.UgitServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tindo.UgitServer.dll"]
