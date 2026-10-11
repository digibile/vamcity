# VamCity

VamCity ASP.NET Core application deployment package.

## Linux deployment notes

- Configure the required .NET runtime for the published application.
- Create appsettings.json on the deployment server with the required production settings.
- Configure secrets and database credentials through secure server-side configuration.
- Store certificates outside the public repository and restrict filesystem permissions.
- Configure persistent storage for uploads and logs outside the application package.
- Review the application configuration and database requirements before starting the service.
