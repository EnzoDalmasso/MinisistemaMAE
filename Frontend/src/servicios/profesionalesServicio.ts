import type { BloqueHorario, CrearProfesionalDto, GuardarProfesionalDto, Profesional } from '../modelos/profesional';
import clienteApi from './clienteApi';

export const profesionalesServicio = {
  obtenerTodos: async (): Promise<Profesional[]> => {
    const { data } = await clienteApi.get<Profesional[]>('/profesionales');
    return data;
  },

  crear: async (dto: CrearProfesionalDto): Promise<Profesional> => {
    const { data } = await clienteApi.post<Profesional>('/profesionales', dto);
    return data;
  },

  actualizar: async (id: number, dto: GuardarProfesionalDto): Promise<Profesional> => {
    const { data } = await clienteApi.put<Profesional>(`/profesionales/${id}`, dto);
    return data;
  },

  actualizarDuracionTurno: async (id: number, duracionTurnoMinutos: number): Promise<Profesional> => {
    const { data } = await clienteApi.patch<Profesional>(`/profesionales/${id}/duracion-turno`, { duracionTurnoMinutos });
    return data;
  },

  actualizarHorarios: async (id: number, bloques: BloqueHorario[]): Promise<Profesional> => {
    const { data } = await clienteApi.put<Profesional>(`/profesionales/${id}/horarios`, { bloques });
    return data;
  },

  desactivar: async (id: number): Promise<Profesional> => {
    const { data } = await clienteApi.patch<Profesional>(`/profesionales/${id}/desactivar`);
    return data;
  },

  reactivar: async (id: number): Promise<Profesional> => {
    const { data } = await clienteApi.patch<Profesional>(`/profesionales/${id}/reactivar`);
    return data;
  },

  eliminar: async (id: number): Promise<void> => {
    await clienteApi.delete(`/profesionales/${id}`);
  },
};
