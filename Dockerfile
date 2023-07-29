# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# Copiar o arquivo .csproj para o contêiner
COPY NomeDoProjeto.csproj ./
#RUN dotnet restore --use-current-runtime

COPY . ./
RUN dotnet publish -c Release -o out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Define o comando para iniciar a aplicação
ENTRYPOINT ["dotnet", "YoutubeStreamingAPI.dll"]
