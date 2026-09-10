import { useContext } from 'react';
import { ContextoAutenticacion } from '../contexto/ContextoAutenticacion';

export function useAutenticacion() {
  const contexto = useContext(ContextoAutenticacion);
  if (!contexto) {
    throw new Error('useAutenticacion debe usarse dentro de <ProveedorAutenticacion>.');
  }
  return contexto;
}
