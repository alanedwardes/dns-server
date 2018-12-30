FROM microsoft/dotnet:2.1-sdk

RUN mkdir /opt/dns-server

ADD . /opt/dns-server

RUN dotnet publish /opt/dns-server --configuration Release --runtime linux-x64

EXPOSE 53:53/udp

ENTRYPOINT ["/opt/dns-server/bin/Release/netcoreapp2.1/linux-x64/publish/dns-server"]
