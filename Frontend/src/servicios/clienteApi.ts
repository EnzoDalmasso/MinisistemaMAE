import axios from 'axios';
import { borrarSesion, obtenerSesion } from '../utilidades/almacenamientoSesion';

const clienteApi = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5260/api',
});

// Agrega el token JWT a cada request (si hay una sesión activa). El backend
// sigue siendo la autoridad real de autorización; esto solo evita mandar
// requests sin token cuando ya sabemos que van a fallar.
clienteApi.interceptors.request.use((configuracion) => {
  const sesion = obtenerSesion();
  if (sesion?.token) {
    configuracion.headers.Authorization = `Bearer ${sesion.token}`;
  }
  return configuracion;
});

// Si el backend responde 401 (token vencido o inválido), se limpia la sesión
// local y se manda al usuario de vuelta al login.
clienteApi.interceptors.response.use(
  (respuesta) => respuesta,
  (error) => {
    if (error.response?.status === 401) {
      borrarSesion();
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  },
);

export default clienteApi;
