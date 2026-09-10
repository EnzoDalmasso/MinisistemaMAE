# MiniSistema MAE

Prueba técnica para MAE Software: sistema de gestión de turnos para una
clínica, con 3 roles (Administrador, Profesional y Paciente) y la regla de
negocio central de cualquier sistema de turnos — un profesional no puede
tener dos turnos activos en la misma fecha y horario — resuelta también a
nivel de base de datos, no solo en el código.

## Funcionalidades

- Alta y listado de pacientes (nombre, apellido, teléfono, obra social).
- Alta y listado de profesionales (nombre, apellido, especialidad, y cuenta
  de acceso propia).
- Crear, listar (con filtros), modificar y cancelar turnos.
- Estados de turno: Pendiente, Confirmado, Cancelado, Atendido.
- Login con JWT y autorización por rol, validada en el backend.
- Cada profesional ve únicamente sus propios turnos.
- Paciente autogestionado: pide, reprograma y cancela sus propios turnos
  eligiendo de una grilla de horarios realmente disponibles (funcionalidad
  agregada sobre la consigna original, ver "Roles").

## Tecnologías

**Frontend:** React 19, TypeScript, Vite, Mantine, React Router, Axios.

**Backend:** C# / .NET 10, ASP.NET Core Web API, Entity Framework Core,
FluentValidation, JWT, BCrypt.

**Base de datos:** PostgreSQL (Supabase en producción).

**Deployment:** Vercel (frontend), Render (backend), Supabase (base de
datos).

## Arquitectura

Backend en Clean Architecture, 4 capas:

```
Clinica.Dominio          entidades, enums, interfaces de repositorio
Clinica.Aplicacion       casos de uso, DTOs, validadores, excepciones
Clinica.Infraestructura  EF Core, repositorios, JWT, hashing
Clinica.API              controladores, middleware, Program.cs
```

Las dependencias van de afuera hacia adentro (API → Infraestructura →
Aplicación → Dominio); Dominio no depende de nada. Se usa repository pattern
con interfaces específicas por entidad, sin repositorio genérico ni Unit of
Work aparte (el `DbContext` ya cumple ese rol acá).

El frontend está organizado por responsabilidad: `paginas/` (una pantalla
por archivo), `servicios/` (llamadas a la API), `modelos/` (tipos que
reflejan los DTOs del backend), `contexto/` (sesión), `rutas/` (guardas de
ruta por rol).

## Roles

**Administrador:** gestiona pacientes, profesionales y turnos. Puede
crear, modificar y cancelar cualquier turno.

**Profesional:** ve solo sus propios turnos. Además de visualizarlos, puede
marcarlos como Atendido o Cancelado (esto va un poco más allá de lo pedido
en la consigna, que dice "solamente visualizar" — lo dejé porque tiene
sentido en un sistema real y ya está probado).

**Paciente** (rol agregado, no pedido explícitamente): se autogestiona con
nombre, apellido y DNI, sin contraseña — decisión deliberada para esta
demo, ver "Decisiones técnicas". Pide, reprograma y cancela sus propios
turnos.

## Ejecución local

Requisitos: .NET SDK 10, Node 20+, PostgreSQL (o Docker).

**Base de datos** (con Docker):

```bash
docker compose up -d
```

**Backend:**

```bash
cd Backend
dotnet run --project Clinica.API
```

Aplica migraciones y carga datos de demo automáticamente al iniciar.
Queda en `http://localhost:5260` (Swagger en `/swagger`).

**Frontend:**

```bash
cd Frontend
npm install
cp .env.example .env
npm run dev
```

Queda en `http://localhost:5173`.

## Variables de entorno

**Backend** (ver `Backend/.env.example` para el detalle):

| Variable | Para qué |
|---|---|
| `ConnectionStrings__BaseDeDatos` | Conexión a PostgreSQL |
| `Jwt__Clave` | Firma de los JWT (larga y aleatoria en producción) |
| `ORIGEN_FRONTEND` | Origen permitido por CORS |
| `DatosSemilla__ContrasenaAdministrador` / `DatosSemilla__ContrasenaProfesional` | Contraseñas de los usuarios de demo (opcional) |

En desarrollo local no hace falta configurar nada de esto:
`appsettings.Development.json` ya trae valores de demo listos para usar.
Para pisar algo puntual sin tocar ese archivo (por ejemplo otro puerto de
Postgres), se puede crear `Backend/Clinica.API/appsettings.Local.json`
(gitignoreado, nunca se sube).

**Frontend** (ver `Frontend/.env.example`): `VITE_API_URL`, la URL de la API.

No hay ningún secreto real commiteado en el repo.

## Usuarios de prueba

En un entorno local nuevo (base de datos vacía), el seed crea
automáticamente dos cuentas:

- `administrador` — rol Administrador.
- `profesional` — rol Profesional, vinculado a un profesional de ejemplo.

El administrador puede dar de alta profesionales adicionales en cualquier
momento desde su propio panel, definiendo usuario y contraseña para cada
uno.

Para el panel de Paciente no hace falta usuario ni contraseña: se entra por
`/acceso-paciente` con nombre, apellido y DNI, y se crea la cuenta en el
momento.

El seed también carga profesionales, pacientes y turnos de ejemplo en
distintos estados, para no arrancar con las pantallas vacías en un entorno
local nuevo.

## Prevención de turnos duplicados

Se valida en dos niveles: primero en el backend (devuelve `409` con un
mensaje claro antes de tocar la base), y como red de seguridad final, un
índice único en PostgreSQL sobre profesional + fecha + horario que
descarta los turnos cancelados. Así, aunque dos requests lleguen casi al
mismo tiempo, la base rechaza el segundo insert — no depende solo de la
validación en memoria.

## Decisiones técnicas

- **Sin contraseña para el paciente:** decisión explícita para esta demo
  (solo nombre + apellido + DNI). Implica un riesgo real si alguien más
  sabe el DNI de otra persona; internamente reutiliza la misma
  infraestructura de JWT/BCrypt (el DNI funciona como usuario y
  contraseña), no es un mecanismo paralelo sin hashear.
- **Un turno cancelado libera el horario**, pendiente/confirmado/atendido
  lo bloquean.
- **Estado inicial distinto según quién crea el turno:** el que carga el
  administrador arranca en Pendiente; el que pide el propio paciente queda
  Confirmado directo, porque ya eligió de una grilla con disponibilidad
  real.
- **Sin Unit of Work ni repositorio genérico:** el `DbContext` ya cumple el
  rol del primero, y cada entidad tiene consultas distintas que no
  encajarían bien en un repositorio genérico.
- **Mapeo DTO ↔ entidad manual**, sin AutoMapper — el dominio es chico y no
  lo justifica.

## Uso de inteligencia artificial

Se usó Claude Code como asistente durante todo el desarrollo (arquitectura,
generación y refactor de código, tests, documentación, corrección de bugs).
Todo el resultado fue revisado, probado en vivo y corregido a mano cuando
hizo falta. Detalle completo en [DOCUMENTACION_IA.md](DOCUMENTACION_IA.md).

## Mejoras futuras

- Contraseña real para el login del paciente (la mejora de seguridad más
  importante pendiente).
- Paginación en los listados.
- Refresh tokens y recuperación de contraseña.
- Notificaciones/recordatorios por email.
- Tests de integración end-to-end sobre el pipeline HTTP completo.
