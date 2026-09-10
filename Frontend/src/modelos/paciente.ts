export interface Paciente {
  id: number;
  nombre: string;
  apellido: string;
  telefono: string;
  obraSocial: string;
}

export interface GuardarPacienteDto {
  nombre: string;
  apellido: string;
  telefono: string;
  obraSocial: string;
}
