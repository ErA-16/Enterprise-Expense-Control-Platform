FROM ://microsoft.com AS build
WORKDIR /src

# Copy the csproj file locally and restore dependencies
COPY ["Business_Operations_Expense_Control_Platform.csproj", "./"]
RUN dotnet restore "Business_Operations_Expense_Control_Platform.csproj"

# Copy the rest of the code files
COPY . .

# FIXED: Explicitly target the csproj file so it ignores the missing test project
RUN dotnet publish "Business_Operations_Expense_Control_Platform.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Business_Operations_Expense_Control_Platform.dll"]