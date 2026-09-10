// La API intercambia fechas como "AAAA-MM-DD" y horarios como "HH:mm:ss"
// (DateOnly/TimeOnly de .NET). Estas funciones son puramente de presentación.

export function formatearFecha(fechaIso: string): string {
  const [anio, mes, dia] = fechaIso.split('-');
  return `${dia}/${mes}/${anio}`;
}

export function formatearHorario(horario: string): string {
  return horario.slice(0, 5); // "HH:mm:ss" -> "HH:mm"
}

// Convierte a "AAAA-MM-DD" usando los componentes locales de la fecha (no
// toISOString, que pasa a UTC y puede correr la fecha un día para atrás).
export function fechaAIso(fecha: Date): string {
  const anio = fecha.getFullYear();
  const mes = String(fecha.getMonth() + 1).padStart(2, '0');
  const dia = String(fecha.getDate()).padStart(2, '0');
  return `${anio}-${mes}-${dia}`;
}

export function isoAFecha(fechaIso: string): Date {
  const [anio, mes, dia] = fechaIso.split('-').map(Number);
  return new Date(anio, mes - 1, dia);
}

export function fechaDeHoyIso(): string {
  return fechaAIso(new Date());
}
