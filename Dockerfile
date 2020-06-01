FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS builder

RUN apt-get update
RUN apt-get install -y unzip libunwind8 gettext
ADD . /DiscordAssistant
WORKDIR /DiscordAssistant
RUN dotnet publish --configuration Release

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

# Install cultures (same approach as Alpine SDK image)
RUN apt-get update && apt-get install -y tzdata

# Disable the invariant mode (set in base image)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=builder /DiscordAssistant/DiscordAssistant/bin/Release/netcoreapp3.1/publish/ /DiscordAssistant/
WORKDIR /DiscordAssistant
ENTRYPOINT dotnet DiscordAssistant.dll