import { Badge } from '@mantine/core';
import type { EstadoTurno } from '../modelos/turno';

const COLOR_POR_ESTADO: Record<EstadoTurno, string> = {
  Pendiente: 'yellow',
  Confirmado: 'blue',
  Atendido: 'green',
  Cancelado: 'red',
};

export function EstadoTurnoBadge({ estado }: { estado: EstadoTurno }) {
  return (
    <Badge color={COLOR_POR_ESTADO[estado]} variant="light">
      {estado}
    </Badge>
  );
}
