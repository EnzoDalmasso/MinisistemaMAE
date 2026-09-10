# Mini sistema de gestión de turnos — Clínica

Prueba técnica para MAE Software: un sistema de gestión de turnos para una
clínica, con autenticación por rol (Administrador / Profesional), gestión de
pacientes y profesionales, y la regla de negocio central de todo sistema de
turnos: **un profesional no puede tener dos turnos activos en la misma fecha
y horario**, resuelta también a nivel de base de datos para cubrir
condiciones de carrera.

## Descripción

El sistema resuelve la operación diaria de agendar turnos en una clínica:

- Un **Administrador** gestiona pacientes, profesionales y turnos: puede
  crear, modificar, cancelar y ver la agenda completa.
- Cada **Profesional** inicia sesión y ve únicamente sus propios turnos —
  nunca los de otro profesional, sin importar lo que pida el frontend.

Todos los datos de la demo (pacientes, profesionales, credenciales) son
ficticios.

## Tecnologías

**Frontend:** React 19, TypeScript, Vite, Mantine v9 (UI), React Router,
Axios.

**Backend:** C# / .NET 10, ASP.NET Core Web API, Entity Framework Core 10,
FluentValidation, JWT Bearer, BCrypt.

**Base de datos:** PostgreSQL.

## Arquitectura

El backend sigue **Clean Architecture** en 4 proyectos, cada uno con una
responsabilidad concreta:

```
Backend/
├── Clinica.Dominio/         Entidades, enumeraciones e interfaces de
│                             repositorio. No depende de ninguna otra capa.
├── Clinica.Aplicacion/      Casos de uso (servicios), DTOs, validadores
│                             (FluentValidation) y excepciones de negocio.
│                             Depende solo de Dominio.
├── Clinica.Infraestructura/ EF Core (DbContext, configuraciones,
│                             migraciones, repositorios), hashing de
│                             contraseñas y generación de JWT. Implementa
│                             las interfaces definidas en Dominio/Aplicacion.
├── Clinica.API/              Controladores, middleware de excepciones,
│                             Program.cs (composición de dependencias).
│                             Capa delgada: no contiene lógica de negocio.
└── Clinica.Tests/            Tests xUnit de las reglas críticas.
```

La regla de dependencias es de afuera hacia adentro: `API → Infraestructura →
Aplicacion → Dominio`. Dominio no conoce a nadie; Aplicacion no conoce EF Core
ni ASP.NET Core.

**Por qué esta estructura y no otra:** se evaluó agregar un proyecto
`Clinica.Dominio.Excepciones` separado y un patrón Unit of Work explícito;
ambos se descartaron por sobreingeniería para el tamaño de este dominio (ver
"Decisiones técnicas" más abajo).

El frontend se organiza por responsabilidad, no por feature, para que cada
carpeta tenga un único propósito:

```
Frontend/src/
├── componentes/   Piezas de UI reutilizables entre pantallas
├── paginas/       Una pantalla completa por archivo
├── servicios/      Acceso a la API (un archivo por recurso, todos sobre
│                    un único cliente Axios con interceptores)
├── modelos/        Tipos TypeScript que reflejan los DTOs del backend
├── hooks/          Hooks propios (ej. useAutenticacion)
├── contexto/        Estado global (sesión del usuario)
├── rutas/           Guardas de ruta (RutaProtegida)
└── utilidades/       Funciones puras (formato de fecha, manejo de errores)
```

## Requisitos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) o superior
- [Node.js](https://nodejs.org/) 20 o superior
- PostgreSQL 16 (local, Docker, o un proveedor gestionado como Neon/Render)
- Docker y Docker Compose (opcional, para levantar PostgreSQL localmente sin instalarlo)

## Instalación y ejecución local

### 1. Base de datos

La forma más simple es con Docker Compose (levanta PostgreSQL en el puerto
`5433` del host, para no chocar con una instalación local en el 5432 por
defecto):

```bash
docker compose up -d
```

Si preferís usar una instancia de PostgreSQL que ya tengas corriendo,
simplemente ajustá la cadena de conexión en el paso siguiente.

### 2. Backend

```bash
cd Backend
dotnet restore
dotnet run --project Clinica.API
```

`Clinica.API/appsettings.Development.json` ya trae una configuración de
desarrollo lista para usar (conexión a `localhost:5433`, clave JWT de
desarrollo, contraseñas de los usuarios de demo). **Son valores ficticios
pensados solo para correr el proyecto localmente**, no secretos reales — ver
la sección de variables de entorno para producción.

Al iniciar, la API aplica las migraciones pendientes automáticamente
(`Database.MigrateAsync()`) y carga los datos de demostración si la base
está vacía. No hace falta ningún paso manual adicional.

La API queda disponible en `http://localhost:5260` (Swagger en
`http://localhost:5260/swagger` en modo desarrollo).

### 3. Frontend

```bash
cd Frontend
npm install
cp .env.example .env   # ya viene con la URL de la API local por defecto
npm run dev
```

La aplicación queda disponible en `http://localhost:5173`.

## Variables de entorno

**Backend** (`Backend/Clinica.API/appsettings.json` para producción, o
variables de entorno — ver `Backend/.env.example` para el detalle completo):

| Variable | Descripción |
|---|---|
| `ConnectionStrings__BaseDeDatos` | Cadena de conexión a PostgreSQL |
| `Jwt__Clave` | Clave de firma de los JWT (larga y aleatoria en producción) |
| `Jwt__Emisor` / `Jwt__Audiencia` | Issuer/audience del token |
| `ORIGEN_FRONTEND` | Origen exacto permitido por CORS |
| `DatosSemilla__ContrasenaAdministrador` / `DatosSemilla__ContrasenaProfesional` | Contraseñas de los usuarios de demo del seed |

**Frontend** (`Frontend/.env`, ver `Frontend/.env.example`):

| Variable | Descripción |
|---|---|
| `VITE_API_URL` | URL base de la API (sin barra final), ej. `http://localhost:5260/api` |

Ningún secreto real está commiteado: `appsettings.Development.json` solo
contiene valores de desarrollo/demo, y los `.env.example` documentan qué
variables hay que definir para un despliegue real.

## Migraciones

Ya existe una migración inicial (`MigracionInicial`) con el esquema completo.
Para crear una nueva migración tras modificar el modelo:

```bash
cd Backend
dotnet ef migrations add NombreDeLaMigracion \
  --project Clinica.Infraestructura --startup-project Clinica.API \
  --output-dir Persistencia/Migraciones
```

Para aplicarlas manualmente (normalmente no hace falta, `Program.cs` ya lo
hace al iniciar):

```bash
dotnet ef database update --project Clinica.Infraestructura --startup-project Clinica.API
```

Requiere el CLI de EF Core: `dotnet tool install --global dotnet-ef`.

## Usuarios de prueba

Cargados automáticamente por el seed (ver
`Clinica.Infraestructura/Persistencia/SeedDeDatos.cs`):

| Usuario | Contraseña (desarrollo) | Rol |
|---|---|---|
| `administrador` | `Admin123!` | Administrador |
| `profesional` | `Profesional123!` | Profesional (vinculado a "Laura Gómez") |

Estas contraseñas están definidas en `appsettings.Development.json` y
`docker-compose`/README solo para que el proyecto funcione de entrada en
local. En un despliegue real se sobreescriben con
`DatosSemilla__ContrasenaAdministrador` / `DatosSemilla__ContrasenaProfesional`.

El seed también carga 3 profesionales, 5 pacientes y 6 turnos ficticios en
distintos estados, para que las pantallas no arranquen vacías.

## Funcionalidades implementadas

- Login con JWT, autorización por rol enforced en el backend (no solo
  ocultando botones).
- CRUD de pacientes y profesionales (alta, listado, edición) — Administrador.
- Gestión completa de turnos: crear, listar con filtros (fecha, profesional,
  estado), modificar, cancelar — Administrador.
- Consulta de turnos propios — Profesional (sin acceso a los de otros
  profesionales, verificado tanto en el listado como al pedir un turno por id).
- Regla de disponibilidad de turnos, con manejo explícito de condiciones de
  carrera (ver más abajo).
- Panel con resumen por rol (conteos globales para Administrador, próximos
  turnos para Profesional).
- Validaciones en frontend y backend (el backend es la autoridad final).
- Manejo global de errores con códigos HTTP y mensajes consistentes.
- Tests automatizados de las reglas de negocio críticas.

## Decisiones técnicas

### Arquitectura

Clean Architecture en 4 capas (ver diagrama arriba). Se usa **repository
pattern** con interfaces chicas y específicas por entidad
(`IRepositorioTurnos`, etc.), sin un repositorio genérico — cada entidad
tiene necesidades de consulta distintas (ej. `IRepositorioTurnos` necesita
`ExisteSolapamientoAsync`, que no tiene sentido en un repositorio genérico).

**No se implementa Unit of Work como patrón separado.** El `DbContext` de EF
Core ya cumple ese rol (agrupa cambios y expone `SaveChangesAsync`); cada
operación de este sistema toca un único agregado por vez, así que una capa
adicional de UoW sería una abstracción redundante para este alcance.

El mapeo DTO ↔ entidad es manual (sin AutoMapper): con 4 entidades, una
dependencia extra para esto no se justifica.

### Autenticación y autorización

JWT Bearer, contraseñas hasheadas con BCrypt (salt aleatorio incluido en el
hash, no se administra por separado). El token incluye el rol y, para
profesionales, un claim propio `profesionalId`.

La autorización real está en el backend:
- `[Authorize(Roles = "Administrador")]` en los endpoints exclusivos de admin.
- En `GET /api/turnos` y `GET /api/turnos/{id}`, si el usuario autenticado es
  Profesional, `ServicioTurnos` **ignora cualquier `profesionalId` que venga
  en el filtro** y fuerza el propio (tomado del claim del token, no de un
  parámetro del cliente). Pedir el turno de otro profesional por id devuelve
  403, no un simple filtro silencioso.

El frontend oculta pantallas/botones según el rol solo por UX; si alguien
llama a la API directamente sin los permisos correspondientes, el backend
igual la rechaza.

### Regla de disponibilidad de turnos (la regla crítica)

**Decisión:** un turno con estado `Cancelado` libera el horario; `Pendiente`,
`Confirmado` y `Atendido` lo bloquean. Es decir, se puede volver a agendar un
turno en el mismo profesional/fecha/horario que uno cancelado, pero nunca
duplicar uno activo.

Se implementa en dos niveles, porque "consultar si existe → crear" no alcanza
ante dos requests simultáneos:

1. **Aplicación** (`ServicioTurnos`): antes de crear o modificar, verifica
   disponibilidad y devuelve `409 Conflict` con un mensaje claro
   ("El profesional ya tiene un turno asignado para esa fecha y horario.").
   Esta es la verificación optimista que cubre el caso normal (sin
   concurrencia) con buena experiencia de usuario.
2. **Base de datos** (red de seguridad final): un **índice único parcial**
   sobre `(ProfesionalId, Fecha, Horario)` filtrado por `Estado <> 'Cancelado'`.
   Si dos requests pasan la verificación de la aplicación casi al mismo
   tiempo, el segundo `INSERT`/`UPDATE` es rechazado por PostgreSQL.
   `RepositorioTurnos` detecta esa violación específica (código `23505`,
   nombre de restricción `IX_Turnos_Disponibilidad`) y la traduce al mismo
   `409 Conflict`, nunca a un `500`.

Adicionalmente, `Turno` usa la columna de sistema `xmin` de PostgreSQL como
token de concurrencia optimista (sin agregar una columna propia), para
detectar si dos administradores editan el mismo turno al mismo tiempo
(`DbUpdateConcurrencyException` → también `409`).

### Validaciones

Dos niveles, con el backend como autoridad final:

- **Frontend:** validación inmediata en los formularios (Mantine `useForm`),
  espejando las reglas del backend (obligatoriedad, longitudes máximas,
  formato de teléfono).
- **Backend:** FluentValidation en la capa de Aplicación, invocado desde cada
  servicio antes de tocar el repositorio (no como filtro de infraestructura
  web), así queda testeable y es el único lugar con la regla real. Errores de
  validación se devuelven agrupados por campo (`{ "mensaje": "...", "errores":
  { "Telefono": ["..."] } }`), y el frontend los mapea de vuelta a cada input.
- Existencia de paciente/profesional y disponibilidad del turno se validan en
  el servicio (no en el validador), porque requieren consultar la base.
- El estado del turno es un enum (`Pendiente`, `Confirmado`, `Cancelado`,
  `Atendido`) serializado como texto; el cliente nunca puede enviar un valor
  fuera de ese conjunto — System.Text.Json lo rechaza con 400 antes de llegar
  al controlador, y además `IsInEnum()` lo revalida como defensa en profundidad.

### Manejo de errores

Middleware global (`ManejadorGlobalDeExcepciones`) que traduce las
excepciones de `Clinica.Aplicacion` a códigos HTTP y un cuerpo consistente
`{ "mensaje": "..." }`, sin exponer stack traces. Evita repetir try/catch en
cada controlador.

| Excepción | Código |
|---|---|
| `ExcepcionValidacion` | 400 |
| `ExcepcionCredencialesInvalidas` | 401 |
| `ExcepcionProhibido` | 403 |
| `ExcepcionNoEncontrado` | 404 |
| `ExcepcionConflicto` | 409 |
| Cualquier otra | 500 (se loguea con stack trace; no se expone al cliente) |

### Base de datos

PostgreSQL vía Npgsql. Claves foráneas con `DeleteBehavior.Restrict` (no se
permite borrar un paciente/profesional que ya tiene turnos, preservando el
historial). Índices: único en `Usuario.NombreUsuario`, único parcial en
`Turno(ProfesionalId, Fecha, Horario)` (la regla de disponibilidad), e índice
simple en `Turno.Fecha` para los filtros de listado.

**Cancelación de turnos:** se usa eliminación lógica (cambio de estado a
`Cancelado`), no borrado físico, para conservar el historial — y porque el
enunciado pide poder mostrarlo en el listado con su estado.

### Seguridad

- JWT con expiración configurable (120 min por defecto).
- Contraseñas con BCrypt, nunca en texto plano.
- CORS restringido al origen exacto del frontend (variable de entorno), no a
  `*`.
- El token se guarda en `localStorage` del lado del frontend: es la
  alternativa estándar y pragmática para una SPA sin backend-for-frontend
  propio; queda documentado como punto a mejorar (ver más abajo) con una
  cookie httpOnly + refresh token si el proyecto creciera.
- Sin secretos commiteados: `appsettings.Development.json` solo trae valores
  de desarrollo, `.env.example` documenta qué configurar en producción.

### Performance

`AsNoTracking()` en todas las consultas de solo lectura (listados). Los
DTOs de turnos se arman a partir de `Include(Paciente).Include(Profesional)`
en vez de proyección directa a DTO en el repositorio — una concesión
deliberada: proyectar directamente ahorraría columnas de más, pero
obligaría al repositorio (capa de Dominio/Infraestructura) a conocer tipos
de la capa de Aplicación, invirtiendo la dependencia. Para el volumen de
datos de este sistema el costo extra es despreciable.

## Testing

`Backend/Clinica.Tests` (xUnit) prioriza la regla de negocio crítica, sobre
SQLite en memoria (a diferencia del proveedor `InMemory` de EF Core, sí
aplica restricciones únicas e índices filtrados, dando fidelidad real a la
regla de disponibilidad):

- Crear un turno válido.
- Rechazar un turno duplicado (mismo profesional/fecha/horario).
- Que un turno cancelado libere el horario para reutilizarse.
- Modificar un turno generando conflicto (y que no queden cambios a medias).
- Que editar solo el estado de un turno no dispare un conflicto consigo mismo.
- Que un profesional solo vea sus propios turnos, incluso intentando forzar
  otro por filtro o por id.
- Que el administrador vea los turnos de todos los profesionales.
- Que un estado fuera del enum sea rechazado por el validador.
- Que crear un turno con un paciente inexistente falle con 404.

```bash
cd Backend
dotnet test
```

## Despliegue

Pensado para opciones simples y gratuitas:

- **Backend:** imagen Docker (`Backend/Dockerfile`) en un servicio como
  [Render](https://render.com) (free web service). Variables de entorno
  según `Backend/.env.example`.
- **Base de datos:** PostgreSQL gestionado, por ejemplo
  [Neon](https://neon.tech) (free tier sin expiración) o el propio PostgreSQL
  gratuito de Render.
- **Frontend:** build estático (`npm run build`) en
  [Vercel](https://vercel.com) o Netlify. Ya incluye `Frontend/vercel.json`
  con el rewrite necesario para que las rutas de React Router funcionen en
  refresh/deep-link.

Pasos generales:

1. Crear la base de datos gestionada y copiar su cadena de conexión.
2. Desplegar el backend con esa cadena de conexión y el resto de las
   variables de `Backend/.env.example` (usar una `Jwt__Clave` nueva y
   aleatoria, nunca la de desarrollo).
3. Desplegar el frontend con `VITE_API_URL` apuntando a la URL pública del
   backend.
4. Volver a configurar `ORIGEN_FRONTEND` en el backend con la URL real del
   frontend ya desplegado (CORS es circular: hace falta la URL final de
   ambos lados).

> No se incluyen URLs de un despliegue real en este README porque el
> despliegue no forma parte de esta entrega; los pasos de arriba alcanzan
> para levantarlo en cualquiera de esas plataformas siguiendo esta misma
> guía.

## Mejoras futuras

Funcionalidades fuera de alcance por el límite de tiempo, priorizadas según
impacto:

- Paginación en los listados (hoy no hace falta por el volumen de datos de
  la demo).
- Refresh tokens y recuperación de contraseña.
- Horarios laborales configurables por profesional y duración de turnos
  (hoy el horario de atención es un rango fijo 07:00–21:00 validado
  globalmente).
- Feriados y bloqueo manual de horarios.
- Auditoría de cambios sobre turnos (quién modificó qué y cuándo).
- Notificaciones/recordatorios por email.
- Tests de integración end-to-end (hoy los tests cubren la capa de
  Aplicación/reglas de negocio, no el pipeline HTTP completo) y tests E2E de
  frontend.
- Rate limiting y logging centralizado/observabilidad.
- CI/CD (build + tests en cada push).
- Code splitting del bundle de frontend (Mantine + la app superan hoy los
  500 KB recomendados por Vite; no es un problema para esta demo pero
  escalaría mal en un proyecto más grande).

## Uso de inteligencia artificial

Ver [DOCUMENTACION_IA.md](DOCUMENTACION_IA.md).
