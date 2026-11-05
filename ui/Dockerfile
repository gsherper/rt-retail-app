# Builds the UI with Vite, then serves the static files with nginx.
# nginx also proxies /hub (SignalR, including the WebSocket upgrade) to the
# server, so the browser talks to one origin, as with the Vite dev proxy.

# --- Build ---
FROM node:24-alpine AS build
WORKDIR /app

# Same pnpm major as the lockfile (lockfileVersion 9.0)
RUN npm install --global pnpm@11.21.0

# Install first, so this layer is cached until the dependencies change
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

COPY . .
RUN pnpm build

# --- Serve ---
FROM nginx:1.29-alpine

# Where /hub is proxied to; the default is the compose `server` service.
# nginx's image substitutes it into the template when the container starts
ENV HUB_UPSTREAM=http://server:5001

COPY nginx.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 5000
