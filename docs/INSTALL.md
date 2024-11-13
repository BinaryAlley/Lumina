## Reverse Proxy Setup
### Rate Limiting Headers
The application uses IP-based rate limiting on some endpoints. When deploying behind a reverse proxy, you must forward the 
original client IP, otherwise the "per IP" rate limiting will become global, per any request aimed at those endpoints, regardless of origin:

```text
# Nginx
proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;

# Apache
RemoteIPHeader X-Forwarded-For

# Caddy
reverse_proxy {
    header_up X-Forwarded-For {remote_host}
}
```
