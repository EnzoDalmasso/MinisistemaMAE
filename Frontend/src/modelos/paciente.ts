export interface Paciente {
  id: number;
  nombre: string;
  apellido: string;
  // Nullable: un paciente autogestionado (alta por DNI) no completa estos
  // datos; el que carga el Administrador manualmente sí los tiene.
  telefono: string | null;
  obraSocial: string | null;
  // Solo tiene valor para pacientes autogestionados.
  dni: string | null;
}

// El alta manual del Administrador sigue pidiendo estos 4 campos.
export interface GuardarPacienteDto {
  nombre: string;
  apellido: string;
  telefono: string;
  obraSocial: string;
}
