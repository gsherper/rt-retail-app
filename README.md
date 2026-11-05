# Realtime Customer Management Demo

React UI and .NET server. All data and all create, update and delete
operations go over a single SignalR WebSocket, so changes show up in every
open browser right away.

- ui/  - React, TypeScript, Vite, Redux Toolkit, Ant Design, AG Grid
- server/ - ASP.NET Core (.NET 10) with SignalR, data kept in memory

# App features

- Open the app in two browser windows. Changes made in one show up in the
  other and the changed cells flash.
- New customers and orders appear at the top of the grid when sorted by ID
  descending (the default).
- Double-click a customer to see their orders. Adding an order there fills in
  the customer for you.
- New orders default to sales person 127 "Adam Smith"; you can change it.
- If someone renames a customer while you have an order form open, the name
  updates in your form.
- Stop the server and the status bar turns red. Start it again and the app
  reconnects on its own.


## Run locally

You need the .NET 10 SDK, Node.js 24 and pnpm.

Start the server (runs on localhost:5001):

cd server
dotnet run --project src/Dashboard.Api


In a second terminal, start the UI:

cd ui
pnpm install
pnpm dev


Open http://localhost:5000

## Run with Docker

You need Docker Desktop running. From the project folder:

docker compose up --build

Open http://localhost:5000

