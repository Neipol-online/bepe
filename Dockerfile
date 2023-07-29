# Imagem base que será utilizada
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

# Diretório de trabalho dentro do contêiner
WORKDIR /app

# Copia os arquivos de projeto e restaura as dependências
COPY *.csproj ./
RUN dotnet restore

# Copia todo o código-fonte e compila o projeto
COPY . ./
RUN dotnet publish -c Release -o out

# Imagem base para a execução do aplicativo
FROM mcr.microsoft.com/dotnet/aspnet:5.0

# Define o diretório de trabalho para a imagem em tempo de execução
WORKDIR /app

# Copia os arquivos do diretório "out" da imagem de compilação para a imagem em tempo de execução
COPY --from=build-env /app/out .

# Comando para iniciar o aplicativo quando o contêiner for executado
CMD ["dotnet", "NomeDoProjeto.dll"]
