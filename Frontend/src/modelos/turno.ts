export type EstadoTurno = 'Pendiente' | 'Confirmado' | 'Cancelado' | 'Atendido';

export const ESTADOS_TURNO: EstadoTurno[] = ['Pendiente', 'Confirmado', 'Cancelado', 'Atendido'];

export interface Turno {
  id: number;
  pacienteId: number;
  pacienteNombreCompleto: string;
  profesionalId: number;
  profesionalNombreCompleto: string;
  profesionalEspecialidad: string;
  fecha: string; // formato "AAAA-MM-DD"
  horario: string; // formato "HH:mm:ss"
  estado: EstadoTurno;
  fechaCreacion: string;
}

export interface CrearTurnoDto {
  pacienteId: number;
  profesionalId: number;
  fecha: string;
  horario: string;
}

export interface ActualizarTurnoDto extends CrearTurnoDto {
  estado: EstadoTurno;
}

export interface TurnoFiltro {
  profesionalId?: number;
  fecha?: string;
  estado?: EstadoTurno;
}
