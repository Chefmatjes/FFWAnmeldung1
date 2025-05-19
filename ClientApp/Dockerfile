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
    echo '  echo "Configuring Nginx to use backend: $NGINX_BACKEND_URL"' >> /entrypoint.sh && \
    echo '  # Create upstream configuration' >> /entrypoint.sh && \
    echo '  cat > /etc/nginx/conf.d/upstream.conf << EOF' >> /entrypoint.sh && \
    echo 'upstream server {' >> /entrypoint.sh && \
    echo '    server ffwanmeldung-server.onrender.com:443;' >> /entrypoint.sh && \
    echo '}' >> /entrypoint.sh && \
    echo 'EOF' >> /entrypoint.sh && \
    echo '  cat /etc/nginx/conf.d/upstream.conf' >> /entrypoint.sh && \
    echo 'fi' >> /entrypoint.sh && \
    echo 'exec nginx -g "daemon off;"' >> /entrypoint.sh && \
    chmod +x /entrypoint.sh

CMD ["/entrypoint.sh"] 