FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Since Render's Root Directory drops us directly in this folder, copy the csproj locally
COPY ["Business_Operations_Expense_Control_Platform.csproj", "./"]
RUN dotnet restore "Business_Operations_Expense_Control_Platform.csproj"

# Copy the rest of the code files and compile
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Business_Operations_Expense_Control_Platform.dll"]