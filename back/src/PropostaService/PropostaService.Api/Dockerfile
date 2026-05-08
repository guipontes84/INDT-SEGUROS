FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY src/PropostaService/PropostaService.Api/bin/Release/net8.0/publish/ .
ENTRYPOINT ["dotnet", "PropostaService.Api.dll"]
