# Documentación del uso de Inteligencia Artificial

Este documento describe honestamente cómo se usó IA en el desarrollo de esta
prueba técnica, tal como pide el enunciado.

## Herramientas utilizadas

- **Claude Code** (Anthropic, modelo Claude Sonnet 5) — única herramienta de
  IA utilizada en este proyecto, para diseño de arquitectura, generación de
  código de backend y frontend, tests, y esta misma documentación.

No se usaron otras herramientas (ChatGPT, Copilot, etc.) en este desarrollo.

## Cómo se usó

El punto de partida fue una especificación técnica propia, muy detallada,
escrita antes de generar una sola línea de código: alcance funcional,
reglas de negocio (en particular la regla de disponibilidad de turnos y su
comportamiento ante turnos cancelados), arquitectura por capas, estrategia
de autenticación/autorización, estrategia de validación, estrategia de
manejo de errores, modelo de datos, endpoints, estructura de carpetas y
prioridades por etapa. Esa especificación es, en sí misma, el criterio
técnico detrás del proyecto; se le pidió explícitamente a Claude Code que
antes de programar analizara los requisitos, identificara riesgos técnicos
(la condición de carrera en la regla de disponibilidad, concretamente) y
propusiera diseño antes de implementar.

A partir de ahí, Claude Code se usó como herramienta de ejecución guiada:

- Scaffolding de la solución .NET (4 capas + tests) y del proyecto React/Vite.
- Implementación de entidades, DTOs, validadores, servicios, repositorios,
  controladores y middleware siguiendo la arquitectura acordada.
- Traducción de la regla de disponibilidad a un índice único parcial en
  PostgreSQL y a la traducción de la violación de esa restricción a un
  `409 Conflict` en `RepositorioTurnos`.
- Redacción de los tests de las reglas críticas (xUnit + SQLite en memoria).
- Redacción del `README.md` y de esta documentación.

## Prompts principales

El prompt inicial (arquitectura, backend, frontend, base de datos, tests,
validaciones y documentación, todo en un mismo mensaje) pedía explícitamente:

- Analizar requisitos y reglas de negocio antes de programar, y presentar un
  plan de arquitectura, modelo de datos, endpoints y etapas de implementación
  para validar antes de escribir código (Claude Code entró en "modo plan" y
  el plan se revisó y aprobó antes de generar el primer archivo).
- Arquitectura limpia en capas (Dominio/Aplicación/Infraestructura/API), sin
  sobreingeniería — evaluando explícitamente qué patrones se justifican
  (repository sí, Unit of Work explícito no) y documentando el porqué.
- Regla de disponibilidad de turnos resuelta a nivel de aplicación **y** de
  base de datos, considerando condiciones de carrera, con la decisión sobre
  si un turno cancelado libera el horario documentada explícitamente.
- Todo el proyecto (identificadores, mensajes, UI, documentación) en español,
  sin `/// <summary>` en los comentarios.
- Prioridad estricta: funcionalidad obligatoria (P1) antes que features
  secundarias, dentro de un desarrollo pensado para 48 horas.

Durante la implementación se usaron prompts de seguimiento más puntuales
para resolver problemas concretos que aparecieron al compilar/testear (por
ejemplo: la API de Npgsql para el token de concurrencia `xmin` cambió entre
versiones y hubo que verificar el nombre real del método contra el paquete
instalado en lugar de asumirlo; el paquete `Microsoft.OpenApi` en la versión
resuelta por Swashbuckle reorganizó sus namespaces; SQLite no soporta
generación automática de valores para una columna `xmin`, lo que llevó a
condicionar esa configuración al proveedor de base de datos activo).

### Segunda ronda: rol Paciente

Ya con el sistema de 2 roles funcionando y probado en vivo, se pidió agregar
un tercer panel para que el paciente autogestione sus propios turnos. Antes
de tocar código, Claude Code volvió a entrar en modo plan y primero hizo
preguntas puntuales (no asumió nada) sobre 3 decisiones que cambiaban el
diseño: cómo se dan de alta los pacientes, qué puede hacer cada uno con sus
turnos, y específicamente si el login del paciente iba a pedir contraseña o
no. Esa última respuesta (sin contraseña) generó una decisión de arquitectura
concreta: reutilizar la infraestructura de JWT/BCrypt existente en vez de
abrir un mecanismo de autenticación paralelo, documentada en el README.

## Revisión humana

Antes de entregar esta prueba, quien la presenta debería revisar
puntualmente:

- Que las credenciales y valores en `appsettings.Development.json` y
  `docker-compose.yml` sean efectivamente ficticios y no se reutilicen en
  ningún entorno real.
- El comportamiento de la regla de disponibilidad en un escenario de
  concurrencia real contra PostgreSQL (los tests automatizados la validan
  contra SQLite en memoria, con fidelidad de índice único pero sin ser el
  motor de producción; ver "Mejoras futuras" en el README).
- Los mensajes de validación y el copy general de la interfaz.

En una sesión posterior sí se verificó el recorrido funcional completo
contra PostgreSQL real (login de los 3 roles, alta/reprogramación/
cancelación de turnos, regla de disponibilidad con 409 real, y las
restricciones de permisos entre pacientes y entre profesionales), tanto por
API directa como por la interfaz en el navegador. En el camino se encontraron
y corrigieron 2 bugs reales gracias a esa prueba en vivo: un formato de
respuesta de error inconsistente para fallas de deserialización JSON, y un
cambio de tipo de dato en la librería de fecha del frontend (Mantine v9
entrega el valor como string en vez de `Date`) que hacía fallar
silenciosamente el guardado de un turno.

## Decisiones propias

Quedaron documentadas explícitamente (en el prompt inicial o resueltas
durante el desarrollo y volcadas al README) las decisiones que requerían
criterio técnico, entre ellas:

- Elegir PostgreSQL por sobre SQL Server, por soporte nativo de índices
  únicos parciales (pieza clave de la regla de disponibilidad) y opciones de
  hosting gratuito más simples.
- Que un turno cancelado libere el horario para reutilizarse.
- Que el login del paciente no pida contraseña (solo Nombre + Apellido +
  DNI): decisión explícita, tomada después de que se explicara el riesgo de
  seguridad concreto que implica.
- No implementar Unit of Work como patrón separado (el `DbContext` ya cumple
  ese rol) ni un repositorio genérico (cada entidad tiene necesidades de
  consulta distintas).
- Validar formato/obligatoriedad con FluentValidation en la capa de
  Aplicación (no como filtro web) y dejar existencia/disponibilidad como
  chequeos explícitos en el servicio, por ser asíncronos y dependientes de
  la base.
- Mapeo manual DTO ↔ entidad en vez de AutoMapper, dado el tamaño acotado
  del dominio.
- Guardar el token JWT en `localStorage` del frontend, como alternativa
  pragmática para una SPA sin backend-for-frontend propio.
- Priorización estricta de funcionalidades obligatorias (P1) sobre mejoras
  visuales o features no pedidas, dado el límite de tiempo de la consigna.
