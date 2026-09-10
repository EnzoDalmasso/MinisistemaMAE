export interface Profesional {
  id: number;
  nombre: string;
  apellido: string;
  especialidad: string;
}

export interface GuardarProfesionalDto {
  nombre: string;
  apellido: string;
  especialidad: string;
}
