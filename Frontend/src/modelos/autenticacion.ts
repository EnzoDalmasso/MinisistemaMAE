export type Rol = 'Administrador' | 'Profesional' | 'Paciente';

export interface IniciarSesionDto {
  nombreUsuario: string;
  contrasena: string;
}

// El paciente "accede" sin contraseña: si el DNI ya está registrado entra a
// esa cuenta, si no, se crea en el momento (ver backend: AccederComoPacienteAsync).
export interface AccesoPacienteDto {
  nombre: string;
  apellido: string;
  dni: string;
}

export interface RespuestaAutenticacion {
  token: string;
  expiraEn: string;
  nombreUsuario: string;
  rol: Rol;
  profesionalId: number | null;
  pacienteId: number | null;
  nombreCompleto: string | null;
}
