FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ARG USERNAME=wutang
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# Create the user
RUN groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME

ENV ASPNETCORE_URLS=http://+:80

EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ContosoCrafts.CheckoutProcessor.fsproj", "./"]
RUN dotnet restore "./ContosoCrafts.CheckoutProcessor.fsproj"
COPY . .
WORKDIR /src
RUN dotnet build "ContosoCrafts.CheckoutProcessor.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContosoCrafts.CheckoutProcessor.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $USERNAME
ENTRYPOINT ["dotnet", "ContosoCrafts.CheckoutProcessor.dll"]
