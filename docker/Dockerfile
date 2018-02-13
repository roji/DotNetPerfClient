FROM microsoft/dotnet-nightly:2.1-sdk

ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE true

COPY . DotNetPerfClient
WORKDIR /DotNetPerfClient/DotNetPerfClient
RUN dotnet build -c Release
CMD dotnet run -c Release
