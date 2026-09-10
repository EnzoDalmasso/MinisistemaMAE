export interface Profesional {
  id: number;
  nombre: string;
  apellido: string;
  especialidad: string;
  duracionTurnoMinutos: number;
}

export interface GuardarProfesionalDto {
  nombre: string;
  apellido: string;
  especialidad: string;
  duracionTurnoMinutos: number;
}
