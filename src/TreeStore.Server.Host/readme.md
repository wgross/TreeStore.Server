# TreeStore.Server.Host

## Restricted Access from Localhost

All APIs of the servie are restrcited to 'localhost' using ASPNetCores 'CORS' module. 
Accessing the service using the machines name or IP results in an error:

```
Bad Request
Bad Request - Invalid Hostname
HTTP Error 400. The request hostname is invalid.
```

Using the IP '127.0.0.1' also fails.

