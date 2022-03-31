#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Spid.Cie.OIDC.AspNetCore.WebApp/Spid.Cie.OIDC.AspNetCore.WebApp.csproj", "Spid.Cie.OIDC.AspNetCore.WebApp/"]
RUN dotnet restore "Spid.Cie.OIDC.AspNetCore.WebApp/Spid.Cie.OIDC.AspNetCore.WebApp.csproj"
COPY . .
WORKDIR "/src/Spid.Cie.OIDC.AspNetCore.WebApp"
RUN dotnet build "Spid.Cie.OIDC.AspNetCore.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Spid.Cie.OIDC.AspNetCore.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Spid.Cie.OIDC.AspNetCore.WebApp.dll"]