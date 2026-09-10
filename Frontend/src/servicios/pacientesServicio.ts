import type { GuardarPacienteDto, Paciente } from '../modelos/paciente';
import clienteApi from './clienteApi';

export const pacientesServicio = {
  obtenerTodos: async (): Promise<Paciente[]> => {
    const { data } = await clienteApi.get<Paciente[]>('/pacientes');
    return data;
  },

  crear: async (dto: GuardarPacienteDto): Promise<Paciente> => {
    const { data } = await clienteApi.post<Paciente>('/pacientes', dto);
    return data;
  },

  actualizar: async (id: number, dto: GuardarPacienteDto): Promise<Paciente> => {
    const { data } = await clienteApi.put<Paciente>(`/pacientes/${id}`, dto);
    return data;
  },
};
