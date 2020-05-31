FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS builder

RUN apt-get update
RUN apt-get install -y unzip libunwind8 gettext
ADD . /NewsCrawler
WORKDIR /NewsCrawler
RUN ./cake.sh -target=ProdBuild

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2

ENV connectionString=

COPY --from=builder /NewsCrawler/NewsCrawler/bin/Release/netcoreapp2.2/publish/ /NewsCrawler/
WORKDIR /NewsCrawler
ENTRYPOINT dotnet NewsCrawler.dll -db  ${connectionString}
