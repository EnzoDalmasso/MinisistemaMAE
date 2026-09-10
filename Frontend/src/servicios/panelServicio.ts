import type { ResumenPanel } from '../modelos/panel';
import clienteApi from './clienteApi';

export const panelServicio = {
  obtenerResumen: async (): Promise<ResumenPanel> => {
    const { data } = await clienteApi.get<ResumenPanel>('/panel/resumen');
    return data;
  },
};
