FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY src/ContratacaoService/ContratacaoService.Api/bin/Release/net8.0/publish/ .
ENTRYPOINT ["dotnet", "ContratacaoService.Api.dll"]
