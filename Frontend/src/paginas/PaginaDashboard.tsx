import { Alert, Center, Grid, Loader, Paper, Table, Text, Title } from '@mantine/core';
import { IconAlertCircle, IconCalendarStats, IconCircleCheck, IconCircleX, IconClockHour4 } from '@tabler/icons-react';
import { useEffect, useState } from 'react';
import { EstadoTurnoBadge } from '../componentes/EstadoTurnoBadge';
import { TarjetaEstadistica } from '../componentes/TarjetaEstadistica';
import { useAutenticacion } from '../hooks/useAutenticacion';
import type { ResumenPanel } from '../modelos/panel';
import { panelServicio } from '../servicios/panelServicio';
import { formatearFecha, formatearHorario } from '../utilidades/formato';
import { obtenerMensajeError } from '../utilidades/manejadorErrores';

export function PaginaDashboard() {
  const { sesion } = useAutenticacion();
  const [resumen, setResumen] = useState<ResumenPanel | null>(null);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let activo = true;
    setCargando(true);
    panelServicio
      .obtenerResumen()
      .then((datos) => {
        if (activo) setResumen(datos);
      })
      .catch((err: unknown) => {
        if (activo) setError(obtenerMensajeError(err));
      })
      .finally(() => {
        if (activo) setCargando(false);
      });

    return () => {
      activo = false;
    };
  }, []);

  if (cargando) {
    return (
      <Center h={300}>
        <Loader />
      </Center>
    );
  }

  if (error || !resumen) {
    return (
      <Alert icon={<IconAlertCircle size={16} />} color="red" title="No se pudo cargar el panel">
        {error ?? 'Intente nuevamente más tarde.'}
      </Alert>
    );
  }

  const esProfesional = sesion?.rol === 'Profesional';

  return (
    <>
      <Title order={2} mb="lg">
        {esProfesional ? 'Mi panel' : 'Panel general'}
      </Title>

      <Grid mb="xl">
        <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
          <TarjetaEstadistica titulo="Total de turnos" valor={resumen.totalTurnos} color="gray" icono={IconCalendarStats} />
        </Grid.Col>
        <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
          <TarjetaEstadistica titulo="Pendientes" valor={resumen.turnosPendientes} color="yellow" icono={IconClockHour4} />
        </Grid.Col>
        <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
          <TarjetaEstadistica titulo="Confirmados" valor={resumen.turnosConfirmados} color="blue" icono={IconCircleCheck} />
        </Grid.Col>
        <Grid.Col span={{ base: 12, sm: 6, md: 3 }}>
          <TarjetaEstadistica titulo="Cancelados" valor={resumen.turnosCancelados} color="red" icono={IconCircleX} />
        </Grid.Col>
      </Grid>

      {esProfesional && (
        <Paper withBorder radius="md" p="md">
          <Title order={4} mb="md">
            Mis próximos turnos
          </Title>
          {resumen.proximosTurnos.length === 0 ? (
            <Text c="dimmed" size="sm">
              No tenés turnos próximos.
            </Text>
          ) : (
            <Table striped highlightOnHover>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Paciente</Table.Th>
                  <Table.Th>Fecha</Table.Th>
                  <Table.Th>Horario</Table.Th>
                  <Table.Th>Estado</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {resumen.proximosTurnos.map((turno) => (
                  <Table.Tr key={turno.id}>
                    <Table.Td>{turno.pacienteNombreCompleto}</Table.Td>
                    <Table.Td>{formatearFecha(turno.fecha)}</Table.Td>
                    <Table.Td>{formatearHorario(turno.horario)}</Table.Td>
                    <Table.Td>
                      <EstadoTurnoBadge estado={turno.estado} />
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          )}
        </Paper>
      )}
    </>
  );
}
