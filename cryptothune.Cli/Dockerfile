#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM microsoft/dotnet:2.0-runtime-nanoserver-1909 AS base
WORKDIR /app

FROM microsoft/dotnet:2.0-sdk-nanoserver-1909 AS build
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
