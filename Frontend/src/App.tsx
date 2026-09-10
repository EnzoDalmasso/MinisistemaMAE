import '@mantine/core/styles.css';
import '@mantine/dates/styles.css';
import '@mantine/notifications/styles.css';

import { MantineProvider } from '@mantine/core';
import { ModalsProvider } from '@mantine/modals';
import { Notifications } from '@mantine/notifications';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { DisenioPrincipal } from './componentes/DisenioPrincipal';
import { ProveedorAutenticacion } from './contexto/ContextoAutenticacion';
import { useAutenticacion } from './hooks/useAutenticacion';
import { PaginaDashboard } from './paginas/PaginaDashboard';
import { PaginaLogin } from './paginas/PaginaLogin';
import { PaginaPacientes } from './paginas/PaginaPacientes';
import { PaginaProfesionales } from './paginas/PaginaProfesionales';
import { PaginaTurnos } from './paginas/PaginaTurnos';
import { RutaProtegida } from './rutas/RutaProtegida';

// Si ya hay una sesión activa, /login redirige directo al panel en vez de
// mostrar el formulario de nuevo.
function RutaLogin() {
  const { sesion } = useAutenticacion();
  if (sesion) {
    return <Navigate to="/" replace />;
  }
  return <PaginaLogin />;
}

function App() {
  return (
    <MantineProvider>
      <Notifications position="top-right" />
      <ModalsProvider>
        <ProveedorAutenticacion>
          <BrowserRouter>
            <Routes>
              <Route path="/login" element={<RutaLogin />} />

              <Route element={<RutaProtegida />}>
                <Route element={<DisenioPrincipal />}>
                  <Route index element={<PaginaDashboard />} />
                  <Route path="turnos" element={<PaginaTurnos />} />

                  <Route element={<RutaProtegida rolesPermitidos={['Administrador']} />}>
                    <Route path="pacientes" element={<PaginaPacientes />} />
                    <Route path="profesionales" element={<PaginaProfesionales />} />
                  </Route>
                </Route>
              </Route>

              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </BrowserRouter>
        </ProveedorAutenticacion>
      </ModalsProvider>
    </MantineProvider>
  );
}

export default App;
