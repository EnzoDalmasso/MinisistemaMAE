import { createContext, useMemo, useState, type ReactNode } from 'react';
import { autenticacionServicio } from '../servicios/autenticacionServicio';
import { borrarSesion, guardarSesion, obtenerSesion, type SesionAlmacenada } from '../utilidades/almacenamientoSesion';

interface ContextoAutenticacionValor {
  sesion: SesionAlmacenada | null;
  cargando: boolean;
  iniciarSesion: (nombreUsuario: string, contrasena: string) => Promise<void>;
  cerrarSesion: () => void;
}

// eslint-disable-next-line react-refresh/only-export-components
export const ContextoAutenticacion = createContext<ContextoAutenticacionValor | undefined>(undefined);

export function ProveedorAutenticacion({ children }: { children: ReactNode }) {
  const [sesion, setSesion] = useState<SesionAlmacenada | null>(() => obtenerSesion());
  const [cargando, setCargando] = useState(false);

  const iniciarSesion = async (nombreUsuario: string, contrasena: string) => {
    setCargando(true);
    try {
      const respuesta = await autenticacionServicio.iniciarSesion({ nombreUsuario, contrasena });
      const nuevaSesion: SesionAlmacenada = {
        token: respuesta.token,
        nombreUsuario: respuesta.nombreUsuario,
        rol: respuesta.rol,
        profesionalId: respuesta.profesionalId,
        expiraEn: respuesta.expiraEn,
      };
      guardarSesion(nuevaSesion);
      setSesion(nuevaSesion);
    } finally {
      setCargando(false);
    }
  };

  const cerrarSesion = () => {
    borrarSesion();
    setSesion(null);
  };

  const valor = useMemo(
    () => ({ sesion, cargando, iniciarSesion, cerrarSesion }),
    [sesion, cargando],
  );

  return <ContextoAutenticacion.Provider value={valor}>{children}</ContextoAutenticacion.Provider>;
}
