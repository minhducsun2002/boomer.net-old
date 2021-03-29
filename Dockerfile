FROM mcr.microsoft.com/dotnet/core/sdk:3.1.201-alpine3.11 as build
WORKDIR /app

COPY . .
RUN dotnet publish -c release --runtime linux-musl-x64 --self-contained -o dist/ 

FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine3.11
COPY --from=build /app/dist /app
WORKDIR /app
CMD ./boomer
