import type { Turno } from './turno';

export interface ResumenPanel {
  totalTurnos: number;
  turnosPendientes: number;
  turnosConfirmados: number;
  turnosCancelados: number;
  turnosAtendidos: number;
  proximosTurnos: Turno[];
}
