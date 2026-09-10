import {
  ActionIcon,
  Alert,
  Button,
  Center,
  Group,
  Loader,
  Modal,
  Select,
  Stack,
  Table,
  Text,
  Title,
} from '@mantine/core';
import { DateInput, TimeInput } from '@mantine/dates';
import { useForm } from '@mantine/form';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import { IconAlertCircle, IconCalendarOff, IconEdit, IconPlus } from '@tabler/icons-react';
import { useEffect, useState } from 'react';
import { EstadoTurnoBadge } from '../componentes/EstadoTurnoBadge';
import { useAutenticacion } from '../hooks/useAutenticacion';
import type { Paciente } from '../modelos/paciente';
import type { Profesional } from '../modelos/profesional';
import { ESTADOS_TURNO, type EstadoTurno, type Turno, type TurnoFiltro } from '../modelos/turno';
import { pacientesServicio } from '../servicios/pacientesServicio';
import { profesionalesServicio } from '../servicios/profesionalesServicio';
import { turnosServicio } from '../servicios/turnosServicio';
import { fechaAIso, formatearFecha, formatearHorario, isoAFecha } from '../utilidades/formato';
import { obtenerErroresDeCampo, obtenerMensajeError } from '../utilidades/manejadorErrores';

interface ValoresFormularioTurno {
  pacienteId: string;
  profesionalId: string;
  fecha: Date | null;
  horario: string;
  estado: EstadoTurno;
}

const VALORES_INICIALES: ValoresFormularioTurno = {
  pacienteId: '',
  profesionalId: '',
  fecha: null,
  horario: '',
  estado: 'Pendiente',
};

export function PaginaTurnos() {
  const { sesion } = useAutenticacion();
  const esAdministrador = sesion?.rol === 'Administrador';

  const [turnos, setTurnos] = useState<Turno[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [filtroFecha, setFiltroFecha] = useState<Date | null>(null);
  const [filtroProfesionalId, setFiltroProfesionalId] = useState<string | null>(null);
  const [filtroEstado, setFiltroEstado] = useState<string | null>(null);

  const [pacientes, setPacientes] = useState<Paciente[]>([]);
  const [profesionales, setProfesionales] = useState<Profesional[]>([]);

  const [modalAbierto, setModalAbierto] = useState(false);
  const [turnoEnEdicion, setTurnoEnEdicion] = useState<Turno | null>(null);
  const [guardando, setGuardando] = useState(false);

  const form = useForm<ValoresFormularioTurno>({
    initialValues: VALORES_INICIALES,
    validate: {
      pacienteId: (valor) => (valor ? null : 'Debe seleccionar un paciente.'),
      profesionalId: (valor) => (valor ? null : 'Debe seleccionar un profesional.'),
      fecha: (valor) => (valor ? null : 'La fecha es obligatoria.'),
      horario: (valor) => (valor ? null : 'El horario es obligatorio.'),
    },
  });

  const cargarTurnos = async () => {
    setCargando(true);
    setError(null);
    try {
      const filtro: TurnoFiltro = {};
      if (filtroFecha) filtro.fecha = fechaAIso(filtroFecha);
      if (esAdministrador && filtroProfesionalId) filtro.profesionalId = Number(filtroProfesionalId);
      if (filtroEstado) filtro.estado = filtroEstado as EstadoTurno;

      const datos = await turnosServicio.obtener(filtro);
      setTurnos(datos);
    } catch (err) {
      setError(obtenerMensajeError(err));
    } finally {
      setCargando(false);
    }
  };

  useEffect(() => {
    void cargarTurnos();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filtroFecha, filtroProfesionalId, filtroEstado]);

  useEffect(() => {
    if (!esAdministrador) return;
    pacientesServicio.obtenerTodos().then(setPacientes).catch(() => undefined);
    profesionalesServicio.obtenerTodos().then(setProfesionales).catch(() => undefined);
  }, [esAdministrador]);

  const abrirModalCrear = () => {
    setTurnoEnEdicion(null);
    form.setValues(VALORES_INICIALES);
    form.clearErrors();
    setModalAbierto(true);
  };

  const abrirModalEditar = (turno: Turno) => {
    setTurnoEnEdicion(turno);
    form.setValues({
      pacienteId: String(turno.pacienteId),
      profesionalId: String(turno.profesionalId),
      fecha: isoAFecha(turno.fecha),
      horario: formatearHorario(turno.horario),
      estado: turno.estado,
    });
    form.clearErrors();
    setModalAbierto(true);
  };

  const manejarEnvio = form.onSubmit(async (valores) => {
    setGuardando(true);
    try {
      const horario = valores.horario.length === 5 ? `${valores.horario}:00` : valores.horario;
      const datosComunes = {
        pacienteId: Number(valores.pacienteId),
        profesionalId: Number(valores.profesionalId),
        fecha: fechaAIso(valores.fecha as Date),
        horario,
      };

      if (turnoEnEdicion) {
        await turnosServicio.actualizar(turnoEnEdicion.id, { ...datosComunes, estado: valores.estado });
        notifications.show({ color: 'green', message: 'Turno actualizado correctamente.' });
      } else {
        await turnosServicio.crear(datosComunes);
        notifications.show({ color: 'green', message: 'Turno creado correctamente.' });
      }
      setModalAbierto(false);
      await cargarTurnos();
    } catch (err) {
      const erroresDeCampo = obtenerErroresDeCampo(err);
      if (Object.keys(erroresDeCampo).length > 0) {
        form.setErrors(erroresDeCampo);
      } else {
        // Acá cae, entre otros casos, el 409 de la regla de disponibilidad.
        notifications.show({ color: 'red', message: obtenerMensajeError(err) });
      }
    } finally {
      setGuardando(false);
    }
  });

  const manejarCancelar = (turno: Turno) => {
    modals.openConfirmModal({
      title: 'Cancelar turno',
      children: (
        <Text size="sm">
          ¿Confirmás cancelar el turno de <strong>{turno.pacienteNombreCompleto}</strong> con{' '}
          <strong>{turno.profesionalNombreCompleto}</strong> el {formatearFecha(turno.fecha)} a las{' '}
          {formatearHorario(turno.horario)}? Esta acción no se puede deshacer.
        </Text>
      ),
      labels: { confirm: 'Cancelar turno', cancel: 'Volver' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        try {
          await turnosServicio.cancelar(turno.id);
          notifications.show({ color: 'green', message: 'Turno cancelado correctamente.' });
          await cargarTurnos();
        } catch (err) {
          notifications.show({ color: 'red', message: obtenerMensajeError(err) });
        }
      },
    });
  };

  const opcionesPacientes = pacientes.map((p) => ({ value: String(p.id), label: `${p.nombre} ${p.apellido}` }));
  const opcionesProfesionales = profesionales.map((p) => ({
    value: String(p.id),
    label: `${p.nombre} ${p.apellido} — ${p.especialidad}`,
  }));
  const opcionesEstado = ESTADOS_TURNO.map((estado) => ({ value: estado, label: estado }));

  return (
    <>
      <Group justify="space-between" mb="lg">
        <Title order={2}>{esAdministrador ? 'Turnos' : 'Mis turnos'}</Title>
        {esAdministrador && (
          <Button leftSection={<IconPlus size={16} />} onClick={abrirModalCrear}>
            Nuevo turno
          </Button>
        )}
      </Group>

      <Group mb="md" align="flex-end">
        <DateInput
          label="Fecha"
          placeholder="Todas las fechas"
          value={filtroFecha}
          onChange={(valor) => setFiltroFecha(valor ? new Date(valor) : null)}
          clearable
          w={180}
        />
        {esAdministrador && (
          <Select
            label="Profesional"
            placeholder="Todos los profesionales"
            data={opcionesProfesionales}
            value={filtroProfesionalId}
            onChange={setFiltroProfesionalId}
            clearable
            searchable
            w={260}
          />
        )}
        <Select
          label="Estado"
          placeholder="Todos los estados"
          data={opcionesEstado}
          value={filtroEstado}
          onChange={setFiltroEstado}
          clearable
          w={180}
        />
      </Group>

      {error && (
        <Alert icon={<IconAlertCircle size={16} />} color="red" mb="md" title="No se pudo cargar el listado">
          {error}
        </Alert>
      )}

      {cargando ? (
        <Center h={200}>
          <Loader />
        </Center>
      ) : (
        <Table striped highlightOnHover withTableBorder>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Paciente</Table.Th>
              <Table.Th>Profesional</Table.Th>
              <Table.Th>Fecha</Table.Th>
              <Table.Th>Horario</Table.Th>
              <Table.Th>Estado</Table.Th>
              {esAdministrador && <Table.Th w={100} />}
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {turnos.length === 0 ? (
              <Table.Tr>
                <Table.Td colSpan={esAdministrador ? 6 : 5}>
                  <Text c="dimmed" ta="center" py="md">
                    No hay turnos para los filtros seleccionados.
                  </Text>
                </Table.Td>
              </Table.Tr>
            ) : (
              turnos.map((turno) => (
                <Table.Tr key={turno.id}>
                  <Table.Td>{turno.pacienteNombreCompleto}</Table.Td>
                  <Table.Td>
                    {turno.profesionalNombreCompleto}
                    <Text size="xs" c="dimmed">
                      {turno.profesionalEspecialidad}
                    </Text>
                  </Table.Td>
                  <Table.Td>{formatearFecha(turno.fecha)}</Table.Td>
                  <Table.Td>{formatearHorario(turno.horario)}</Table.Td>
                  <Table.Td>
                    <EstadoTurnoBadge estado={turno.estado} />
                  </Table.Td>
                  {esAdministrador && (
                    <Table.Td>
                      <Group gap={4} wrap="nowrap">
                        <ActionIcon variant="subtle" onClick={() => abrirModalEditar(turno)} aria-label="Editar turno">
                          <IconEdit size={16} />
                        </ActionIcon>
                        {turno.estado !== 'Cancelado' && (
                          <ActionIcon
                            variant="subtle"
                            color="red"
                            onClick={() => manejarCancelar(turno)}
                            aria-label="Cancelar turno"
                          >
                            <IconCalendarOff size={16} />
                          </ActionIcon>
                        )}
                      </Group>
                    </Table.Td>
                  )}
                </Table.Tr>
              ))
            )}
          </Table.Tbody>
        </Table>
      )}

      <Modal
        opened={modalAbierto}
        onClose={() => setModalAbierto(false)}
        title={turnoEnEdicion ? 'Editar turno' : 'Nuevo turno'}
      >
        <form onSubmit={manejarEnvio}>
          <Stack>
            <Select
              label="Paciente"
              placeholder="Seleccionar paciente"
              required
              searchable
              data={opcionesPacientes}
              {...form.getInputProps('pacienteId')}
            />
            <Select
              label="Profesional"
              placeholder="Seleccionar profesional"
              required
              searchable
              data={opcionesProfesionales}
              {...form.getInputProps('profesionalId')}
            />
            <DateInput
              label="Fecha"
              placeholder="Seleccionar fecha"
              required
              minDate={turnoEnEdicion ? undefined : new Date()}
              {...form.getInputProps('fecha')}
            />
            <TimeInput label="Horario" required {...form.getInputProps('horario')} />
            {turnoEnEdicion && (
              <Select
                label="Estado"
                required
                data={opcionesEstado}
                {...form.getInputProps('estado')}
              />
            )}
            <Group justify="flex-end" mt="sm">
              <Button variant="default" onClick={() => setModalAbierto(false)}>
                Cancelar
              </Button>
              <Button type="submit" loading={guardando}>
                Guardar
              </Button>
            </Group>
          </Stack>
        </form>
      </Modal>
    </>
  );
}
