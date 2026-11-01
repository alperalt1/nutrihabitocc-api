# Nutrihabitocc API

API .NET 9 para autenticación, workspaces, pacientes, historias clínicas,
recetas, plantillas de menú, menús mensuales y catálogo de productos.

## Estructura

```
Nutrihabitocc/
  Api/              controladores, DTOs de entrada y configuración
  Domain/           entidades, servicios, puertos y reglas de negocio
  Infraestructure/  EF Core, repositorios, migraciones y almacenamiento S3
Dockerfile          imagen de producción
appsettings.example.json  variables que se configuran en producción
```

## Desarrollo local

```bash
dotnet restore
ASPNETCORE_ENVIRONMENT=Development dotnet run --project Nutrihabitocc/Nutrihabitocc.csproj
```

La API aplica las migraciones y crea los roles/administrador inicial al arrancar.
La configuración local vive en `Nutrihabitocc/appsettings.Development.json`, que
**no se sube al repositorio**.

## Endpoints de la app móvil (solo lectura)

La app móvil se autentica con `platform=mobile` y rol `Paciente`.

- `POST /api/Auth/login`
- `POST /api/Auth/refresh`
- `GET /api/Auth/me`
- `GET /api/MenuMensual/mio?anio=2026&mes=10` — menú publicado (opcional `workspaceId`)
- `GET /api/MenuMensual/mio/menus?anio=2026&mes=10` — menús publicados de todos sus workspaces
- `GET /api/MenuMensual/mio/dias/2026-10-15` — día publicado (opcional `workspaceId`)
- `GET /api/ClinicalHistory/paciente/{userId}` — historias propias
- `GET /api/ClinicalHistory/{id}` — detalle de una historia propia

El menú incluye `recetaSnapshotJson` con nombre, instrucciones e ingredientes de
cada receta, por lo que el móvil no necesita llamar a los endpoints de staff.

## Producción

Consulta `DEPLOY.md`.
