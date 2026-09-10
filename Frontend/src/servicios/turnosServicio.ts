import type {
  ActualizarTurnoDto,
  CambiarEstadoTurnoDto,
  CrearTurnoDto,
  ReprogramarTurnoDto,
  Turno,
  TurnoFiltro,
} from '../modelos/turno';
import clienteApi from './clienteApi';

export const turnosServicio = {
  obtener: async (filtro: TurnoFiltro): Promise<Turno[]> => {
    const parametros = new URLSearchParams();
    if (filtro.profesionalId) parametros.set('profesionalId', String(filtro.profesionalId));
    if (filtro.fecha) parametros.set('fecha', filtro.fecha);
    if (filtro.estado) parametros.set('estado', filtro.estado);

    const { data } = await clienteApi.get<Turno[]>('/turnos', { params: parametros });
    return data;
  },

  obtenerPorId: async (id: number): Promise<Turno> => {
    const { data } = await clienteApi.get<Turno>(`/turnos/${id}`);
    return data;
  },

  crear: async (dto: CrearTurnoDto): Promise<Turno> => {
    const { data } = await clienteApi.post<Turno>('/turnos', dto);
    return data;
  },

  actualizar: async (id: number, dto: ActualizarTurnoDto): Promise<Turno> => {
    const { data } = await clienteApi.put<Turno>(`/turnos/${id}`, dto);
    return data;
  },

  cancelar: async (id: number): Promise<Turno> => {
    const { data } = await clienteApi.patch<Turno>(`/turnos/${id}/cancelar`);
    return data;
  },

  reprogramar: async (id: number, dto: ReprogramarTurnoDto): Promise<Turno> => {
    const { data } = await clienteApi.put<Turno>(`/turnos/${id}/reprogramar`, dto);
    return data;
  },

  cambiarEstado: async (id: number, dto: CambiarEstadoTurnoDto): Promise<Turno> => {
    const { data } = await clienteApi.patch<Turno>(`/turnos/${id}/estado`, dto);
    return data;
  },
};
