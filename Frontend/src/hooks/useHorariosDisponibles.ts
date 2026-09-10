import { useEffect, useState } from 'react';
import { turnosServicio } from '../servicios/turnosServicio';

// Trae la grilla de horarios libres de un profesional en una fecha (según su
// duración de turno configurada) cada vez que cambian esos dos datos.
// "excluirTurnoId" evita que el horario actual de un turno que se está
// reprogramando/editando desaparezca de la lista por estar "ocupado" por sí mismo.
export function useHorariosDisponibles(
  profesionalId: string | number | null | undefined,
  fecha: string | null | undefined,
  excluirTurnoId?: number,
) {
  const [horarios, setHorarios] = useState<string[]>([]);
  const [cargando, setCargando] = useState(false);

  useEffect(() => {
    if (!profesionalId || !fecha) {
      setHorarios([]);
      return;
    }

    let activo = true;
    setCargando(true);
    turnosServicio
      .obtenerHorariosDisponibles(Number(profesionalId), fecha, excluirTurnoId)
      .then((datos) => {
        if (activo) setHorarios(datos);
      })
      .catch(() => {
        if (activo) setHorarios([]);
      })
      .finally(() => {
        if (activo) setCargando(false);
      });

    return () => {
      activo = false;
    };
  }, [profesionalId, fecha, excluirTurnoId]);

  return { horarios, cargando };
}
