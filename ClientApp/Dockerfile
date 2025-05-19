FROM node:18-alpine AS build
WORKDIR /app

# Copy package files and install dependencies
COPY package.json package-lock.json* ./
RUN npm install

# Copy the rest of the files
COPY . .

# Build the app
RUN npm run build

# Serve with Nginx
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Create a healthcheck endpoint
RUN mkdir -p /usr/share/nginx/html/health && \
    echo "OK" > /usr/share/nginx/html/health/index.html

EXPOSE 80

# Create entrypoint script
RUN echo '#!/bin/sh' > /entrypoint.sh && \
    echo 'if [ -n "$NGINX_BACKEND_URL" ]; then' >> /entrypoint.sh && \
    echo '  echo "Replacing server:8080 with $NGINX_BACKEND_URL"' >> /entrypoint.sh && \
    echo '  sed -i "s|server:8080|https://$NGINX_BACKEND_URL|g" /etc/nginx/conf.d/default.conf' >> /entrypoint.sh && \
    echo 'fi' >> /entrypoint.sh && \
    echo 'exec nginx -g "daemon off;"' >> /entrypoint.sh && \
    chmod +x /entrypoint.sh

CMD ["/entrypoint.sh"] 