# 1. Use the official Microsoft SDK image for .NET 10 to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Business_Operations_Expense_Control_Platform.csproj", "./"]
RUN dotnet restore "Business_Operations_Expense_Control_Platform.csproj"

# Copy the rest of the source code and build it
COPY . .
RUN dotnet publish "Business_Operations_Expense_Control_Platform.csproj" -c Release -o /app/publish

# 2. Use the official ASP.NET Core runtime image for .NET 10 for deployment
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy the compiled output from the build stage
COPY --from=build /app/publish .

# Define the entry point to run your application
ENTRYPOINT ["dotnet", "Business_Operations_Expense_Control_Platform.dll"]
