export type Rol = 'Administrador' | 'Profesional';

export interface IniciarSesionDto {
  nombreUsuario: string;
  contrasena: string;
}

export interface RespuestaAutenticacion {
  token: string;
  expiraEn: string;
  nombreUsuario: string;
  rol: Rol;
  profesionalId: number | null;
}
