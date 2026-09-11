export interface Paciente {
  id: number;
  nombre: string;
  apellido: string;
  // Nullable: un paciente autogestionado (alta por DNI) no completa estos
  // datos hasta pedir su primer turno; el que carga el Administrador
  // manualmente sí los tiene (salvo email, que es exclusivo del autoservicio).
  telefono: string | null;
  obraSocial: string | null;
  email: string | null;
  // Solo tiene valor para pacientes autogestionados.
  dni: string | null;
}

// El alta manual del Administrador sigue pidiendo estos 4 campos. El DNI es
// opcional y solo se muestra/envía al editar (permite corregir uno mal
// cargado por un paciente autogestionado); vacío lo limpia.
export interface GuardarPacienteDto {
  nombre: string;
  apellido: string;
  telefono: string;
  obraSocial: string;
  dni: string;
}

// DTO acotado para el autoservicio del paciente (no permite tocar
// nombre/apellido/DNI, a diferencia de GuardarPacienteDto).
export interface ActualizarContactoPacienteDto {
  telefono: string;
  obraSocial: string;
  email: string;
}
