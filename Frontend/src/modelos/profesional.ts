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

// Solo se usa al dar de alta: el administrador define acá el usuario y
// contraseña con los que ese profesional va a loguearse (ver
// ServicioProfesionales.CrearAsync en el backend, que crea el Usuario
// vinculado en el mismo paso).
export interface CrearProfesionalDto extends GuardarProfesionalDto {
  nombreUsuario: string;
  contrasena: string;
}
