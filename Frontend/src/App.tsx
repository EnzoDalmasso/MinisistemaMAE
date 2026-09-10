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
import { PaginaAccesoPaciente } from './paginas/PaginaAccesoPaciente';
import { PaginaDashboard } from './paginas/PaginaDashboard';
import { PaginaLogin } from './paginas/PaginaLogin';
import { PaginaMisTurnos } from './paginas/PaginaMisTurnos';
import { PaginaPacientes } from './paginas/PaginaPacientes';
import { PaginaProfesionales } from './paginas/PaginaProfesionales';
import { PaginaTurnos } from './paginas/PaginaTurnos';
import { RutaProtegida } from './rutas/RutaProtegida';

// Si ya hay una sesión activa, /login y /acceso-paciente redirigen directo
// al panel en vez de mostrar el formulario de nuevo.
function RutaLogin() {
  const { sesion } = useAutenticacion();
  if (sesion) {
    return <Navigate to="/" replace />;
  }
  return <PaginaLogin />;
}

function RutaAccesoPaciente() {
  const { sesion } = useAutenticacion();
  if (sesion) {
    return <Navigate to="/" replace />;
  }
  return <PaginaAccesoPaciente />;
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
              <Route path="/acceso-paciente" element={<RutaAccesoPaciente />} />

              <Route element={<RutaProtegida />}>
                <Route element={<DisenioPrincipal />}>
                  <Route index element={<PaginaDashboard />} />

                  <Route element={<RutaProtegida rolesPermitidos={['Administrador', 'Profesional']} />}>
                    <Route path="turnos" element={<PaginaTurnos />} />
                  </Route>

                  <Route element={<RutaProtegida rolesPermitidos={['Paciente']} />}>
                    <Route path="mis-turnos" element={<PaginaMisTurnos />} />
                  </Route>

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
