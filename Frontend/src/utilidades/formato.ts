// La API intercambia fechas como "AAAA-MM-DD" y horarios como "HH:mm:ss"
// (DateOnly/TimeOnly de .NET). Estas funciones son puramente de presentación.
//
// No hay conversión Date <-> string acá a propósito: los componentes de fecha
// de Mantine (@mantine/dates v9) ya trabajan con strings "AAAA-MM-DD"
// directamente, que es el mismo formato que espera el backend — convertir a
// Date de por medio solo agregaba una fuente de bugs (ver PaginaTurnos.tsx).

export function formatearFecha(fechaIso: string): string {
  const [anio, mes, dia] = fechaIso.split('-');
  return `${dia}/${mes}/${anio}`;
}

export function formatearHorario(horario: string): string {
  return horario.slice(0, 5); // "HH:mm:ss" -> "HH:mm"
}
