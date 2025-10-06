FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VelsatBackendAPI/VelsatMobile.csproj", "VelsatBackendAPI/"]
COPY ["VelsatBackendAPI.Data/VelsatMobile.Data.csproj", "VelsatBackendAPI.Data/"]
COPY ["VelsatBackendAPI.Model/VelsatMobile.Model.csproj", "VelsatBackendAPI.Model/"]
RUN dotnet restore "VelsatBackendAPI/VelsatMobile.csproj"
COPY . .
WORKDIR "/src/VelsatBackendAPI"
RUN dotnet build "VelsatMobile.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VelsatMobile.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VelsatMobile.dll"]