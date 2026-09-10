import { Navigate, Outlet } from 'react-router-dom';
import { useAutenticacion } from '../hooks/useAutenticacion';
import type { Rol } from '../modelos/autenticacion';

interface Props {
  rolesPermitidos?: Rol[];
}

// La restricción real de qué puede ver/hacer cada rol vive en el backend
// (autorización por endpoint). Esto es solo para no renderizar pantallas que
// de todas formas el backend va a rechazar, mejorando la experiencia de uso.
export function RutaProtegida({ rolesPermitidos }: Props) {
  const { sesion } = useAutenticacion();

  if (!sesion) {
    return <Navigate to="/login" replace />;
  }

  if (rolesPermitidos && !rolesPermitidos.includes(sesion.rol)) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}
