import type { IniciarSesionDto, RespuestaAutenticacion } from '../modelos/autenticacion';
import clienteApi from './clienteApi';

export const autenticacionServicio = {
  iniciarSesion: async (dto: IniciarSesionDto): Promise<RespuestaAutenticacion> => {
    const { data } = await clienteApi.post<RespuestaAutenticacion>('/autenticacion/iniciar-sesion', dto);
    return data;
  },
};
