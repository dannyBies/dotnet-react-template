# dotnet-react-template

My preferred tech stack which I use for my personal projects. This is currently only designed to run in a local development environment.

I've designed it so that everything is available in a single assembly and I have grouped related classes together which should make it easier to follow the code flow. In real projects it's most likely better to split this into different projects.

The following features still need to implemented:

- Scalability
  - Scaling out multiple MassTransit servers
  - SignalR backplane
- Code quality
  - Split program.cs into extension methods
  - Add roslyn analyzers
- Authorization
  - Add roles/groups to API and SignalR
- CI/CD
  - Add github actions to deploy BE
  - Add vercel integration to deploy FE
- Database
  - Add indexes
  - Add DataAnnotations

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
- ShadCN
- Auth.js

## Prerequisites

### Ngrok

We will need this so Auth0 can call our API that's running locally.
Create an account and make sure you have a static tunnel, e.g. https://sheep-equipped-weirdly.ngrok-free.app. We will use this in our Auth0 config so make sure this url is always the same. Run `ngrok config edit` and paste in the following

```yaml
authtoken: <your auth token>
version: 2
tunnels:
  example:
    proto: http
    hostname: <your_ngrok_url>
    addr: https://127.0.0.1:7082
```

You can now run `ngrok start example` which will make your API available on your ngrok url when it's running.

### Auth0

Create a new Single Page Application on Auth0. In the `Allowed Callback URLs` section add these urls, `http://localhost:3000/api/auth/callback/auth0, <your_ngrok_url>/api/auth/callback/auth0`.

In the `Allowed Logout URLs` section add these urls, `http://localhost:3000, <your_ngrok_url>`.

Once you've done this create a new Api with the default values.

Finally we are going to create a new `Post Login` action which we will use to call our own API to register a user in our database, and use the generated user id from our database as a custom claim that will be used to authenticate/authorize the user in our backend. You can find this script [here](./Scripts/PostLogin.ts). Make sure to update the `<ngrok-url>` in this script to your own url.

### Docker

We are using RabbitMQ which we'll use from within a docker container, make sure you have Docker Desktop up and running. Alternatively you could try installing it on your machine without Docker.

### Database

Make sure your MSSQL database is running with a database named `example` in it.

## Run the app

### Backend

In the `appsettings.json`, replace `<your-domain>` with the Auth0 domain specified within the Single Page Application Settings. Replace `<your-audience>` with the API audience value from the API you created earlier.

Run `dotnet ef database update` to run the migrations and create the user table in your database.

### Frontend

Create an `.env.local` file in the root of the FE project, it should contain the following:

```
AUTH0_CLIENT_ID="<client-id>"
AUTH0_CLIENT_SECRET="<client-secret>"
AUTH0_ISSUER_BASE_URL="<issuer>"
AUTH0_AUDIENCE="<audience>"

NEXTAUTH_URL="http://localhost:3000"
NEXTAUTH_SECRET=<generate string with openssl rand -base64 32>
```

> Note: These secrets are not exposed to the client, they're only used server-side!

You can retrieve the `<client-id>` and `<client-secret>` from the Auth0 application. `<issuer>` is the Auth0 domain prefixed with `https://`. `<audience>` is the same as the value you used in the `appsettings.json`

### Run

I use a [bat file](./Scripts/startServices.bat) to automatically spin up Ngrok and RabbitMQ, and open the FE and BE so we can easily start developing. Make sure to replace `<FE-location>` and `<BE-location>` in this script to point to the location of your BE and FE.

In the BE tab you can run `dotnet run`

In the FE tab you run `yarn install` and afterwards `yarn dev`.

## Architecture

When a user goes to the app they will need to register and login through Auth0. When this is done Auth0 will call our create user endpoint. This endpoint will then use `MassTransit` to send a `CreateUser` message. This will then be handled by the `CreateUserConsumer` who will then add the user to the database. Once this is done we let the controller know the message was processed correctly and we also publish another message `UserCreated`. The controller returns the newly generated userId so that Auth0 can use this to add it as a claim inside the accessToken. This claim is then used by our API and `SignalR` to check if the user is authorized. The `UserCreatedConsumer` then uses `SignalR` to message every connected user to let them know a new user is connected.

When a message is published from a controller we automatically add a `InitiatedByUser` header which contains the userId of the user. This is propagated throughout the system. If you publish a new message from within a consumer it will still pass through the `InitiatedByUser` header so you will have full traceability.

If an exception is thrown from within a Consumer it will retry 3 times unless it's an `InvalidOperationException`. If it keeps failing it will eventually be moved to an error queue. If the message came from a MassTransit RequestClient an `RequestFaultException` will be thrown and caught by an Exception Filter which returns a BadRequest to our client.
