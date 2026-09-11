import type { ActualizarContactoPacienteDto, GuardarPacienteDto, Paciente } from '../modelos/paciente';
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

  eliminar: async (id: number): Promise<void> => {
    await clienteApi.delete(`/pacientes/${id}`);
  },

  // Autoservicio del paciente logueado (rol Paciente): siempre opera sobre
  // su propia ficha, nunca recibe un id.
  obtenerMiPerfil: async (): Promise<Paciente> => {
    const { data } = await clienteApi.get<Paciente>('/pacientes/mi-perfil');
    return data;
  },

  actualizarMiContacto: async (dto: ActualizarContactoPacienteDto): Promise<Paciente> => {
    const { data } = await clienteApi.put<Paciente>('/pacientes/mi-perfil', dto);
    return data;
  },
};
