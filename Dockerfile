FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder
WORKDIR /src

COPY ./Sources/Fanex.PendingCommand ./Fanex.PendingCommand
COPY ./Sources/Fanex.PendingCommand.Service/Fanex.PendingCommand.Service.csproj ./Fanex.PendingCommand.Service/Fanex.PendingCommand.Service.csproj

WORKDIR /src/Fanex.PendingCommand.Service
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN --mount=type=cache,target=/root/.nuget/packages \ 
	dotnet restore -v n -r linux-x64

COPY ./Sources/Fanex.PendingCommand.Service .

RUN --mount=type=cache,target=/root/.nuget/packages \
	dotnet publish -r linux-x64 --self-contained false -c Release -o /app 

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-alpine AS base

WORKDIR /app
COPY --from=builder /app .

ENV TZ="America/La_Paz"
ENV COMPlus_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "Fanex.PendingCommand.Service.dll"]



