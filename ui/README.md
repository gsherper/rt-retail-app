Realtime Customer Management App Demo

pnpm dev to start running on localhost:5000

The UI expects the server to be running on localhost:5001
vite dev server proxy the /hub SignalR Websocket requests to localhost:5001

All data over one single Websocket.
Data CRUD operations flow through Websocket.
Changes are any data cell will appear on screen and grid cell will flash

Customer List page, when new customers are added it will appear at the beginning fo the grid if CustomerID is sort DESC

Double-clicking on customer row will take you to the Order list page filtered to that specific customer. Adding orders will prefill the customer detail in the order form.

Order List page, when new orders are added it will appear at the beginning fo the grid if OrderID is sort DESC

All order form creation will prepopulate with default Sales Person DEFAULT_SALES_PERSON_ID 127 "David Cheung", but it can be changed by the user when they fill out the form.

If at any time another user is using the app changes a customer name and you have an order form in progress, the customer name will update.
