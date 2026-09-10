// Forma de las respuestas de error que devuelve el middleware global del backend.
export interface ErrorApi {
  mensaje: string;
  errores?: Record<string, string[]>;
}
