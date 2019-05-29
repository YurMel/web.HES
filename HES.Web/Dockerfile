FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["HES.Web/HES.Web.csproj", "HES.Web/"]
COPY ["HES.Infrastructure/HES.Infrastructure.csproj", "HES.Infrastructure/"]
COPY ["HES.Core/HES.Core.csproj", "HES.Core/"]
RUN dotnet restore "HES.Web/HES.Web.csproj"
COPY . .
WORKDIR "/src/HES.Web"
RUN dotnet build "HES.Web.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "HES.Web.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "HES.Web.dll"]