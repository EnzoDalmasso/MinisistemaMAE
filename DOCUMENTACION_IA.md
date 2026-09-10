# Uso de inteligencia artificial

## Herramienta

Claude Code (Anthropic, Claude Sonnet 5), única herramienta de IA usada en
este proyecto.

## Cómo se usó

Definí yo el alcance funcional, las reglas de negocio (la regla de
disponibilidad de turnos y su comportamiento con turnos cancelados en
particular), la arquitectura por capas, la estrategia de autenticación y
los roles, antes de escribir código. Le pedí a Claude Code que primero
analizara eso, identificara riesgos técnicos (la condición de carrera al
crear un turno, concretamente) y propusiera un plan de implementación antes
de tocar código.

A partir de ahí lo usé para:

- Armar el scaffolding del backend (4 capas + tests) y del frontend.
- Implementar entidades, DTOs, validadores, servicios, controladores y
  middleware siguiendo la arquitectura acordada.
- Resolver la regla de disponibilidad con un índice único parcial en
  PostgreSQL, y traducir su violación a un error 409 legible.
- Escribir los tests de las reglas críticas.
- Redactar el README y esta documentación.
- Agregar después el rol Paciente: antes de tocar código, me preguntó
  puntualmente cómo se daba de alta un paciente, qué podía hacer con sus
  turnos y si el login iba a pedir contraseña — esa última respuesta (sin
  contraseña) es mía, quedó documentada como decisión explícita en el README.
- Una auditoría final antes de esta entrega, revisando seguridad,
  requisitos de la consigna, configuración de producción y documentación.

## Qué se revisó y corrigió

Probé el sistema completo en vivo (los 3 roles, alta/edición/cancelación de
turnos, el 409 de turno duplicado, permisos entre pacientes y entre
profesionales) contra PostgreSQL real, no solo los tests automatizados. En
el camino aparecieron y se corrigieron bugs reales, entre ellos:

- Un formato de error inconsistente cuando el JSON llegaba mal formado.
- Un bug de tipos en el frontend (`@mantine/dates` v9 cambió el tipo que
  devuelve el selector de fecha) que rompía en silencio el guardado de un
  turno.
- En la auditoría final: un bug donde, si se desplegaba sin configurar las
  contraseñas de demo por variable de entorno, el sistema quedaba
  inaccesible en producción en vez de usar el valor por defecto esperado.

## Decisiones que tomé yo

- Postgres en vez de SQL Server, por los índices únicos parciales.
- Que un turno cancelado libere el horario.
- Que el paciente no tenga contraseña, después de que se me explicara el
  riesgo.
- No usar Unit of Work ni repositorio genérico, ni AutoMapper — el dominio
  es chico y no lo justifica.
- Priorizar lo obligatorio de la consigna por sobre features extra, dado el
  tiempo disponible.

El resto del trabajo (escribir el código siguiendo esas decisiones, generar
los tests, la documentación) fue asistido por IA y revisado por mí antes de
cada commit.
