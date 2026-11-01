# Despliegue de la API .NET

## Variables de entorno

En producción se configuran variables de entorno; no se sube
`appsettings.Development.json`.

| Variable | Ejemplo |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:8421` |
| `ConnectionStrings__DefaultConnection` | `Host=...;Database=nutrihabitocc;Username=...;Password=...;SSL Mode=Require` |
| `Jwt__Key` | Clave larga y aleatoria |
| `Jwt__Issuer` | `MiApiLocal` |
| `Jwt__Audience` | `MiApiUsuarios` |
| `Cors__AllowedOrigins__0` | `https://app.tudominio.com` |
| `BootstrapAdmin__Email` | `admin@tudominio.com` |
| `BootstrapAdmin__Password` | Clave inicial |
| `BootstrapAdmin__Nombres` | `Administrador` |
| `BootstrapAdmin__Apellidos` | `Sistema` |
| `S3__Endpoint` | Endpoint S3-compatible |
| `S3__Bucket` | `assets` |
| `S3__Region` | `us-east-2` |
| `S3__AccessKey` | Credencial |
| `S3__SecretKey` | Credencial |
| `Api__PublicBaseUrl` | `https://api.tudominio.com` |
| `Resent__Host` | `smtp.resend.com` |
| `Resent__Port` | `587` |
| `Resent__UserName` | `resend` |
| `Resent__Password` | Clave SMTP |
| `Resent__From` | `soporte@tudominio.com` |

`BootstrapAdmin` solo crea el administrador si todavía no existe un usuario con
ese rol. Después del primer despliegue puedes rotar la contraseña desde la web.

## Docker

```bash
docker build -t nutrihabit-api .
docker run --rm -p 8421:8421 --env-file .env nutrihabit-api
```

La API aplica las migraciones de EF Core al iniciar. El endpoint de salud es
`GET /health`.

## Dokploy

1. Crea una aplicación en el mismo proyecto `nutrihabit`.
2. Source: GitHub, repositorio de la API, branch `main`.
3. Build Type: Dockerfile, path `Dockerfile`, context `.`.
4. Container Port: `8421`.
5. Agrega el dominio de la API con HTTPS.
6. Configura las variables de entorno de la tabla.
7. Health check: `GET /health`.
8. Deploy.

La API debe desplegarse después del servicio de scraping para que las tablas de
productos existan, aunque la API no depende de Python en tiempo de ejecución.
