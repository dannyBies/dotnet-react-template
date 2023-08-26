# dotnet-react-template

This repository contains my preferred tech stack for personal projects. Currently, it's optimized for a local development environment.

I've structured the code to ensure everything is accessible from a single assembly, with related classes bundled together for clarity. However, for bigger projects, consider dividing the code into separate projects.

The following features still need to implemented:

- Code quality
  - Refactor program.cs using extension methods
  - Add roslyn analyzers
- Authorization
  - Add roles/groups for API and SignalR
- CI/CD
  - Set up GitHub actions for backend deployment
  - Integrate Vercel for frontend deployment
- Database
  - Implement indexes
  - Integrate DataAnnotations

Backend Tech used:

- Auth0
- Ngrok
- ASP.NET Core
- FluentValidation
- MassTransit
- SignalR
- EFCore
- MSSQL
- Serilog

Frontend Tech used:

- React
- Next.js
- TailwindCSS
- Shadcn
- Auth.js

## Setup

### Ngrok

- Ngrok facilitates Auth0 to interact with our locally hosted API.
- Register for an account and obtain a persistent tunnel, such as https://sheep-equipped-weirdly.ngrok-free.app. This URL should remain consistent for the Auth0 configuration.
- Run ngrok config edit and input:

```yaml
authtoken: <your auth token>
version: 2
tunnels:
  example:
    proto: http
    hostname: <your_ngrok_url>
    addr: https://127.0.0.1:7082
```

Run `ngrok start example`.

### Auth0

- Create a new Single Page Application.
- Under Allowed Callback URLs, add: `http://localhost:3000/api/auth/callback/auth0` and `<your_ngrok_url>/api/auth/callback/auth0`.
- For Allowed Logout URLs, include: `http://localhost:3000` and `<your_ngrok_url>`.
- Set up a new API with default parameters.
- Create a Post Login action to register users in our database and assign a unique user ID as a custom claim for backend authentication. Update the script located [here](./Scripts/PostLogin.ts) with your Ngrok URL.

### Docker

- We utilize RabbitMQ within a Docker container. Make sure Docker Desktop is running. Alternatively, consider a manual installation.

### Database

- Make sure your MSSQL database is running with a database named `example` in it.

## Run the app

### Backend

- In appsettings.json, substitute `<your-domain>` with the domain from Auth0's Single Page Application Settings. Replace `<your-audience>` with the audience value from your newly created API.
- Execute dotnet ef database update.

### Frontend

Create an `.env.local` in the frontend project's root with:

```
AUTH0_CLIENT_ID="<client-id>"
AUTH0_CLIENT_SECRET="<client-secret>"
AUTH0_ISSUER_BASE_URL="<issuer>"
AUTH0_AUDIENCE="<audience>"

NEXTAUTH_URL="http://localhost:3000"
NEXTAUTH_SECRET=<generate string with openssl rand -base64 32>
```

> Note: These secrets remain server-side and aren't exposed to the client.

Retrieve values like <client-id>, <client-secret>, and others from the Auth0 dashboard.

### Run

- Use the [provided script](./Scripts/startServices.bat) to initiate Ngrok, RabbitMQ, and both frontend and backend. make sure to update `<FE-location>` and `<BE-location>` paths in the script.
- For the backend, run `dotnet run`.
- For the frontend, first run `yarn install`, then `yarn dev`

## Architecture

In the app, users can register and login via Auth0. Post-registration, Auth0 communicates with our user creation endpoint. This endpoint then uses MassTransit to broadcast a `CreateUser` message, which `CreateUserConsumer` processes by adding the user to our database. Once successful, the controller acknowledges the successful message processing and broadcasts another message, `UserCreated`. The controller then sends back the newly generated userId, which Auth0 uses as a claim within the accessToken. This claim is then used for authentication and authorization by our API and SignalR. The `UserCreatedConsumer` then notifies all connected SignalR users about the new user.

Messages published from controllers automatically include an `InitiatedByUser` header which includes the generated userId, maintaining traceability throughout the system.

Errors within a Consumer lead to three retry attempts, Except when an `InvalidOperationException` exception happens. Persistent failures result in the message moving to an error queue. If the message originates from a MassTransit RequestClient, an `RequestFaultException` is triggered and intercepted by an Exception Filter, resulting in a BadRequest response to the client.

For user feedback regarding process failures, implement an `IConsumer<Fault<CreateUser>>` and use the `InitiatedBy` header to notify the user via `SignalR`.
