import type { Rol } from '../modelos/autenticacion';

const CLAVE_ALMACENAMIENTO = 'clinica_sesion';

export interface SesionAlmacenada {
  token: string;
  nombreUsuario: string;
  rol: Rol;
  profesionalId: number | null;
  expiraEn: string;
}

// Único punto de lectura/escritura de la sesión en localStorage: lo usan tanto
// el contexto de autenticación como el interceptor de axios, para no duplicar
// la clave de almacenamiento en dos lugares distintos.
export function obtenerSesion(): SesionAlmacenada | null {
  const valor = localStorage.getItem(CLAVE_ALMACENAMIENTO);
  if (!valor) {
    return null;
  }

  try {
    return JSON.parse(valor) as SesionAlmacenada;
  } catch {
    return null;
  }
}

export function guardarSesion(sesion: SesionAlmacenada): void {
  localStorage.setItem(CLAVE_ALMACENAMIENTO, JSON.stringify(sesion));
}

export function borrarSesion(): void {
  localStorage.removeItem(CLAVE_ALMACENAMIENTO);
}
