# Giai đoạn 1: Build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Sao chép file .csproj và khôi phục dependencies
COPY *.csproj ./
RUN dotnet restore

# Sao chép toàn bộ mã nguồn và build
COPY . ./
RUN dotnet publish -c Release -o /app

# Giai đoạn 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

# Sao chép ứng dụng từ giai đoạn build
COPY --from=build /app .

# Mở cổng ứng dụng
EXPOSE 5184

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "Motocom.dll"]
