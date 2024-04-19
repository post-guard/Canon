FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt update
RUN apt install libfontconfig1 -y
WORKDIR /App
COPY bin/Release/net8.0/publish .
ENTRYPOINT ["dotnet", "Canon.Server.dll"]
