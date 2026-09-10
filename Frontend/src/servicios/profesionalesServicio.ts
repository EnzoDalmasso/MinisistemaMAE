import type { CrearProfesionalDto, GuardarProfesionalDto, Profesional } from '../modelos/profesional';
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
};
