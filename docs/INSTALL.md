## General Setup
In the `appsettings.json` file from the **API server** app, make sure there is an entry looking like this:
```json
"CorsSettings": {
    "AllowedOrigins": [
        "http://localhost:5012"
    ]
}
```
where the `AllowedOrigins` section includes the address set in the `Kestrel` section of the `appsettings.json` filefrom the **Web Client** app:
```json
"Kestrel": {
    "Endpoints": {
        "Http": {
            "Url": "http://localhost:5012"
        }
    }
}
```
The above values are the defaults of Lumina, but you can change them to whatever host and port you desire, as long as they are both the same, in both files. Failing to do so will result in SignalR not being able to make a connection between the client app and the server API.

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
