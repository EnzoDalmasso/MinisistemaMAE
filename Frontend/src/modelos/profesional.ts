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
