FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the csproj file using the folder path from the monorepo context
COPY ["Business_Operations_Expense_Control_Platform/Business_Operations_Expense_Control_Platform.csproj", "Business_Operations_Expense_Control_Platform/"]
RUN dotnet restore "Business_Operations_Expense_Control_Platform/Business_Operations_Expense_Control_Platform.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Business_Operations_Expense_Control_Platform"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# FIXED: Standardized assembly name format matching your project dll
ENTRYPOINT ["dotnet", "Business_Operations_Expense_Control_Platform.dll"]
