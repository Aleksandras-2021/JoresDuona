# JoresDuona

## Building and Running the application

### 1. Clone the repository
First, clone the repository to your local machine:

```bash
git clone https://github.com/Aleksandras-2021/JoresDuona
```
### 2. Install .NET SDK

Ensure you have the .NET 8 or higher SDK version installed on your system. You can download it from [here](https://dotnet.microsoft.com/download).

### 3. Navigate to the main project folder (JoresDuona)

```
cd JoresDuona
```

### 4. Restore dependencies

```bash
dotnet restore
```
### 5. Build the application

```bash
dotnet build
```

### 6. To run the server locally:

```bash
cd posApi
dotnet run
```

### 7. To run the client locally:

```bash
cd posClient
dotnet run
```

## Database setup

### Setup docker container

#### 1. Start the Docker container

```bash
cd posApi
docker-compose up -d
```

#### 2. Stop and remove Docker container

```bash
docker-compose down -v
```

### Migrations

#### 1. Update the database with migrations

To apply migrations to the database, run the following command:

```bash
cd posapi
dotnet ef database update
```

#### 2. Adding a New Migration

To add a new migration, run the following command:

```bash
dotnet ef migrations add <MigrationName>
```

#### 3. If you need to remove the last migration, you can use:

```bash
dotnet ef migrations remove
```

### Database Connection information for Local Development

The database connection information  is stored in the appsettings.Development.json file. You do not need to modify anything as it is already created

Example:

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;example;Username=example;Password=example;Database=example;Pooling=true;"
  }
}
```
### Structure
Program is divided into 3 projects `PosClient` `PosApi` `PosShared`
 - `PosClient` is responsible for frontend.
 - `PosServer` is responsible for backend
 - `PosShared` holds classes,models that both client and server can use.


### Authorization
Program uses cookies to authorize users. Admin account is automatically created on start.
Details
email: `admin@gmail.com`
password: `admin`
### How to send cookie with bearer details(Client)
Cookies store information of users `email`, `id`, `role`. If you need to perform some validation, this can be one of the many approaches.
```csharp
//Use `ApiService` for POST/GET/PUT/DELETE, example:
`var response = await _apiService.GetAsync(apiUrl);`
```

### How to receive cookie with bearer details(Server)
Cookies store information of users `email`, `id`, `role`. If you need to perform some validation, this can be one of the many approaches.
```csharp
//Use UserTokenService to extract user from token, example:
`User? sender = await _userTokenService.GetUserFromTokenAsync();`
```
