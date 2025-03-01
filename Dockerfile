FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/KogaseEngine.Api/KogaseEngine.Api.csproj", "src/KogaseEngine.Api/"]
COPY ["src/KogaseEngine.Core/KogaseEngine.Core.csproj", "src/KogaseEngine.Core/"]
COPY ["src/KogaseEngine.Domain/KogaseEngine.Domain.csproj", "src/KogaseEngine.Domain/"]
COPY ["src/KogaseEngine.Infra/KogaseEngine.Infra.csproj", "src/KogaseEngine.Infra/"]
COPY ["Directory.Packages.props", "."]
RUN dotnet restore "src/KogaseEngine.Api/KogaseEngine.Api.csproj"
COPY . .
WORKDIR "/src/src/KogaseEngine.Api"
RUN dotnet build "KogaseEngine.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KogaseEngine.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KogaseEngine.Api.dll"]