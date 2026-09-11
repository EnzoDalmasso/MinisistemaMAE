// Coincide con el enum DayOfWeek de .NET: el backend lo serializa como texto
// en inglés (JsonStringEnumConverter), empezando por domingo.
export type DiaSemana = 'Sunday' | 'Monday' | 'Tuesday' | 'Wednesday' | 'Thursday' | 'Friday' | 'Saturday';

export const DIAS_SEMANA: { valor: DiaSemana; etiqueta: string }[] = [
  { valor: 'Monday', etiqueta: 'Lunes' },
  { valor: 'Tuesday', etiqueta: 'Martes' },
  { valor: 'Wednesday', etiqueta: 'Miércoles' },
  { valor: 'Thursday', etiqueta: 'Jueves' },
  { valor: 'Friday', etiqueta: 'Viernes' },
  { valor: 'Saturday', etiqueta: 'Sábado' },
  { valor: 'Sunday', etiqueta: 'Domingo' },
];

export interface BloqueHorario {
  diaSemana: DiaSemana;
  // "HH:mm:ss", tal cual serializa TimeOnly en el backend.
  horaInicio: string;
  horaFin: string;
}

export interface Profesional {
  id: number;
  nombre: string;
  apellido: string;
  especialidad: string;
  duracionTurnoMinutos: number;
  email: string | null;
  activo: boolean;
  // Calculado por el backend: desactivado hace 7+ días y sin turnos
  // asociados. Controla si se puede mostrar el botón "Eliminar".
  puedeEliminarse: boolean;
  // Días y horarios en los que atiende. Vacío significa "sin horario propio
  // configurado" (el backend cae en ese caso al horario general de la
  // clínica, ver ServicioTurnos en el backend).
  horarios: BloqueHorario[];
}

export interface GuardarProfesionalDto {
  nombre: string;
  apellido: string;
  especialidad: string;
  duracionTurnoMinutos: number;
  // Opcional: en la edición, dejarlo vacío no cambia el email actual (se
  // manda tal cual, incluso vacío, a diferencia de nuevaContrasena).
  email: string;
  // Opcional: dejarlo vacío en la edición no cambia la contraseña actual.
  nuevaContrasena?: string;
}

// Solo se usa al dar de alta: el administrador define acá el usuario y
// contraseña con los que ese profesional va a loguearse (ver
// ServicioProfesionales.CrearAsync en el backend, que crea el Usuario
// vinculado en el mismo paso).
export interface CrearProfesionalDto extends GuardarProfesionalDto {
  nombreUsuario: string;
  contrasena: string;
}
