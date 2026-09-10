import { AxiosError } from 'axios';
import type { ErrorApi } from '../modelos/errorApi';

const MENSAJE_GENERICO = 'Ocurrió un error inesperado. Intente nuevamente más tarde.';

export function obtenerMensajeError(error: unknown): string {
  if (error instanceof AxiosError) {
    const datos = error.response?.data as ErrorApi | undefined;
    if (datos?.mensaje) {
      return datos.mensaje;
    }
  }
  return MENSAJE_GENERICO;
}

// El diccionario "errores" del backend viene con los nombres de propiedad de
// C# (PascalCase, ej. "ObraSocial"), porque la política camelCase de
// serialización solo afecta a propiedades reflejadas, no a claves de
// diccionario. Se convierten a camelCase para poder usarlos directamente con
// form.setErrors() de Mantine, cuyos campos usan esa convención.
export function obtenerErroresDeCampo(error: unknown): Record<string, string> {
  if (!(error instanceof AxiosError)) {
    return {};
  }

  const datos = error.response?.data as ErrorApi | undefined;
  if (!datos?.errores) {
    return {};
  }

  const resultado: Record<string, string> = {};
  for (const [campo, mensajes] of Object.entries(datos.errores)) {
    const campoCamelCase = campo.charAt(0).toLowerCase() + campo.slice(1);
    resultado[campoCamelCase] = mensajes[0];
  }
  return resultado;
}
