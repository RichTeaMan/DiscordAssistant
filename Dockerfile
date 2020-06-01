FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS builder

RUN apt-get update
RUN apt-get install -y unzip libunwind8 gettext
ADD . /DiscordAssistant
WORKDIR /DiscordAssistant
RUN dotnet publish --configuration Release

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine3.11

# Install cultures (same approach as Alpine SDK image)
RUN apk add --no-cache icu-libs

# Disable the invariant mode (set in base image)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENV jenkins:username=
ENV jenkins:key=
ENV discord:token=

COPY --from=builder /DiscordAssistant/DiscordAssistant/bin/Release/netcoreapp3.1/publish/ /DiscordAssistant/
WORKDIR /DiscordAssistant
ENTRYPOINT dotnet DiscordAssistant.dll