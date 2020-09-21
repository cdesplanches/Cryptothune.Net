FROM microsoft/dotnet:3.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:3.1-sdk AS build
WORKDIR /src
COPY ["cryptothune.Cli/cryptothune.Cli.csproj", "cryptothune.Cli/"]
RUN dotnet restore "cryptothune.Cli/cryptothune.Cli.csproj"
COPY . .
WORKDIR "/src/cryptothune.Cli"
RUN dotnet build "cryptothune.Cli.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "cryptothune.Cli.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "cryptothune.Cli.dll"]
