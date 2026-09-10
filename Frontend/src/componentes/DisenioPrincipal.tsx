import { Avatar, AppShell, Box, Burger, Button, Group, NavLink, Text } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { IconCalendarEvent, IconLayoutDashboard, IconLogout, IconStethoscope, IconUsers } from '@tabler/icons-react';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAutenticacion } from '../hooks/useAutenticacion';

export function DisenioPrincipal() {
  const [navAbierta, { toggle }] = useDisclosure();
  const { sesion, cerrarSesion } = useAutenticacion();
  const navegar = useNavigate();
  const ubicacion = useLocation();

  const esAdministrador = sesion?.rol === 'Administrador';
  const esPaciente = sesion?.rol === 'Paciente';

  // Para un paciente, "nombreUsuario" es su DNI: se muestra su nombre real
  // en su lugar cuando está disponible.
  const nombreAMostrar = sesion?.nombreCompleto ?? sesion?.nombreUsuario ?? '';

  const manejarCerrarSesion = () => {
    cerrarSesion();
    navegar(esPaciente ? '/acceso-paciente' : '/login', { replace: true });
  };

  const enlaces = [
    { etiqueta: 'Panel', ruta: '/', icono: IconLayoutDashboard, visible: true },
    { etiqueta: 'Pacientes', ruta: '/pacientes', icono: IconUsers, visible: esAdministrador },
    { etiqueta: 'Profesionales', ruta: '/profesionales', icono: IconStethoscope, visible: esAdministrador },
    { etiqueta: 'Turnos', ruta: '/turnos', icono: IconCalendarEvent, visible: !esPaciente },
    { etiqueta: 'Mis turnos', ruta: '/mis-turnos', icono: IconCalendarEvent, visible: esPaciente },
  ].filter((enlace) => enlace.visible);

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{ width: 240, breakpoint: 'sm', collapsed: { mobile: !navAbierta } }}
      padding="md"
    >
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between">
          <Group>
            <Burger opened={navAbierta} onClick={toggle} hiddenFrom="sm" size="sm" />
            <Text fw={700}>Clínica · Gestión de turnos</Text>
          </Group>

          <Group gap="sm">
            <Avatar radius="xl" color="blue">
              {nombreAMostrar.charAt(0).toUpperCase()}
            </Avatar>
            <Box visibleFrom="xs">
              <Text size="sm" fw={500} lh={1.2}>
                {nombreAMostrar}
              </Text>
              <Text size="xs" c="dimmed" lh={1.2}>
                {sesion?.rol}
              </Text>
            </Box>
            <Button variant="subtle" color="red" leftSection={<IconLogout size={16} />} onClick={manejarCerrarSesion}>
              Cerrar sesión
            </Button>
          </Group>
        </Group>
      </AppShell.Header>

      <AppShell.Navbar p="md">
        {enlaces.map((enlace) => (
          <NavLink
            key={enlace.ruta}
            label={enlace.etiqueta}
            leftSection={<enlace.icono size={18} />}
            active={ubicacion.pathname === enlace.ruta}
            onClick={() => navegar(enlace.ruta)}
            variant="filled"
          />
        ))}
      </AppShell.Navbar>

      <AppShell.Main bg="var(--mantine-color-gray-0)">
        <Outlet />
      </AppShell.Main>
    </AppShell>
  );
}
