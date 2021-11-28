FROM mcr.microsoft.com/dotnet/runtime:5.0

RUN mkdir /opt/galeriya

ADD build/linux-x64 /opt/galeriya

VOLUME ["/data"]

ENTRYPOINT ["/opt/galeriya/Ae.Galeriya.Web"]
