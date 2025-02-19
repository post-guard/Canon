FROM mcr.microsoft.com/dotnet/aspnet:9.0
RUN apt update
RUN apt install libfontconfig1 -y
WORKDIR /App
COPY bin/Release/net9.0/publish .
ENTRYPOINT ["dotnet", "Canon.Server.dll"]
