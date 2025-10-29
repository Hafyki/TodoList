
FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /back

COPY  . .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

CMD [ "dotnet", "run" ]