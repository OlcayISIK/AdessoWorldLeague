FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY AdessoWorldLeague.slnx .
COPY AdessoWorldLeague.Core/AdessoWorldLeague.Core.csproj AdessoWorldLeague.Core/
COPY AdessoWorldLeague.Mongo/AdessoWorldLeague.Mongo.csproj AdessoWorldLeague.Mongo/
COPY AdessoWorldLeague.Data/AdessoWorldLeague.Data.csproj AdessoWorldLeague.Data/
COPY AdessoWorldLeague.Dto/AdessoWorldLeague.Dto.csproj AdessoWorldLeague.Dto/
COPY AdessoWorldLeague.Repository/AdessoWorldLeague.Repository.csproj AdessoWorldLeague.Repository/
COPY AdessoWorldLeague.Business/AdessoWorldLeague.Business.csproj AdessoWorldLeague.Business/
COPY AdessoWorldLeague.WebApi/AdessoWorldLeague.WebApi.csproj AdessoWorldLeague.WebApi/

RUN dotnet restore AdessoWorldLeague.slnx

COPY . .
RUN dotnet publish AdessoWorldLeague.WebApi/AdessoWorldLeague.WebApi.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AdessoWorldLeague.WebApi.dll"]
