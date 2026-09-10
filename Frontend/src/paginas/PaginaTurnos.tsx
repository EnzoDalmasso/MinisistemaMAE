import {
  ActionIcon,
  Alert,
  Button,
  Center,
  Group,
  Loader,
  Modal,
  NumberInput,
  Select,
  Stack,
  Table,
  Text,
  Title,
} from '@mantine/core';
import { DateInput } from '@mantine/dates';
import { useForm } from '@mantine/form';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import { IconAlertCircle, IconCalendarOff, IconEdit, IconPlus } from '@tabler/icons-react';
import { useEffect, useState } from 'react';
import { EstadoTurnoBadge } from '../componentes/EstadoTurnoBadge';
import { useAutenticacion } from '../hooks/useAutenticacion';
import { useHorariosDisponibles } from '../hooks/useHorariosDisponibles';
import type { Paciente } from '../modelos/paciente';
import type { Profesional } from '../modelos/profesional';
import { ESTADOS_TURNO, type EstadoTurno, type Turno, type TurnoFiltro } from '../modelos/turno';
import { pacientesServicio } from '../servicios/pacientesServicio';
import { profesionalesServicio } from '../servicios/profesionalesServicio';
import { turnosServicio } from '../servicios/turnosServicio';
import { formatearFecha, formatearHorario } from '../utilidades/formato';
import { obtenerErroresDeCampo, obtenerMensajeError } from '../utilidades/manejadorErrores';

interface ValoresFormularioTurno {
  pacienteId: string;
  profesionalId: string;
  // DateInput de @mantine/dates (v9) entrega el valor como string ISO
  // "AAAA-MM-DD" (no como objeto Date), que además es exactamente el formato
  // que espera el backend: no hace falta ninguna conversión intermedia.
  fecha: string | null;
  horario: string | null;
  estado: EstadoTurno;
}

const VALORES_INICIALES: ValoresFormularioTurno = {
  pacienteId: '',
  profesionalId: '',
  fecha: null,
  horario: null,
  estado: 'Pendiente',
};

export function PaginaTurnos() {
  const { sesion } = useAutenticacion();
  const esAdministrador = sesion?.rol === 'Administrador';
  const esProfesional = sesion?.rol === 'Profesional';

  const [turnos, setTurnos] = useState<Turno[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [filtroFecha, setFiltroFecha] = useState<string | null>(null);
  const [filtroProfesionalId, setFiltroProfesionalId] = useState<string | null>(null);
  const [filtroEstado, setFiltroEstado] = useState<string | null>(null);

  const [pacientes, setPacientes] = useState<Paciente[]>([]);
  const [profesionales, setProfesionales] = useState<Profesional[]>([]);

  const [modalAbierto, setModalAbierto] = useState(false);
  const [turnoEnEdicion, setTurnoEnEdicion] = useState<Turno | null>(null);
  const [guardando, setGuardando] = useState(false);

  const [duracionActual, setDuracionActual] = useState<number | null>(null);
  const [duracionInput, setDuracionInput] = useState<number | ''>('');
  const [guardandoDuracion, setGuardandoDuracion] = useState(false);

  const form = useForm<ValoresFormularioTurno>({
    initialValues: VALORES_INICIALES,
    validate: {
      pacienteId: (valor) => (valor ? null : 'Debe seleccionar un paciente.'),
      profesionalId: (valor) => (valor ? null : 'Debe seleccionar un profesional.'),
      fecha: (valor) => (valor ? null : 'La fecha es obligatoria.'),
      horario: (valor) => (valor ? null : 'El horario es obligatorio.'),
    },
  });

  // La grilla de horarios se recalcula sola apenas hay profesional + fecha;
  // "excluirTurnoId" evita que el horario actual del turno en edición
  // desaparezca de la lista por estar "ocupado" por sí mismo.
  const { horarios, cargando: cargandoHorarios } = useHorariosDisponibles(
    form.values.profesionalId,
    form.values.fecha,
    turnoEnEdicion?.id,
  );

  const cargarTurnos = async () => {
    setCargando(true);
    setError(null);
    try {
      const filtro: TurnoFiltro = {};
      if (filtroFecha) filtro.fecha = filtroFecha;
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

  useEffect(() => {
    if (!esProfesional || !sesion?.profesionalId) return;
    profesionalesServicio
      .obtenerTodos()
      .then((lista) => {
        const propio = lista.find((p) => p.id === sesion.profesionalId);
        if (propio) {
          setDuracionActual(propio.duracionTurnoMinutos);
          setDuracionInput(propio.duracionTurnoMinutos);
        }
      })
      .catch(() => undefined);
  }, [esProfesional, sesion?.profesionalId]);

  const manejarGuardarDuracion = async () => {
    if (!sesion?.profesionalId || duracionInput === '') return;
    setGuardandoDuracion(true);
    try {
      const actualizado = await profesionalesServicio.actualizarDuracionTurno(sesion.profesionalId, duracionInput);
      setDuracionActual(actualizado.duracionTurnoMinutos);
      notifications.show({ color: 'green', message: 'Duración de turno actualizada correctamente.' });
    } catch (err) {
      notifications.show({ color: 'red', message: obtenerMensajeError(err) });
    } finally {
      setGuardandoDuracion(false);
    }
  };

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
      fecha: turno.fecha,
      horario: turno.horario,
      estado: turno.estado,
    });
    form.clearErrors();
    setModalAbierto(true);
  };

  const manejarEnvio = form.onSubmit(async (valores) => {
    // Defensa extra por si algún input queda sin confirmar en el estado del
    // formulario pese a pasar la validación de arriba (evita construir un
    // request con datos incompletos en vez de simplemente fallar en silencio).
    if (!valores.fecha || !valores.horario) {
      form.validate();
      return;
    }

    setGuardando(true);
    try {
      const datosComunes = {
        pacienteId: Number(valores.pacienteId),
        profesionalId: Number(valores.profesionalId),
        fecha: valores.fecha,
        horario: valores.horario,
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
      // Se deja en consola para poder diagnosticar errores que no vengan de
      // la API (network, bugs de JS), sin exponer nada al usuario final.
      console.error('Error al guardar el turno:', err);

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

  const manejarCambiarEstado = async (turno: Turno, estado: EstadoTurno) => {
    try {
      await turnosServicio.cambiarEstado(turno.id, { estado });
      notifications.show({ color: 'green', message: 'Estado actualizado correctamente.' });
      await cargarTurnos();
    } catch (err) {
      notifications.show({ color: 'red', message: obtenerMensajeError(err) });
    }
  };

  const opcionesPacientes = pacientes.map((p) => ({ value: String(p.id), label: `${p.nombre} ${p.apellido}` }));
  const opcionesProfesionales = profesionales.map((p) => ({
    value: String(p.id),
    label: `${p.nombre} ${p.apellido} — ${p.especialidad}`,
  }));
  const opcionesEstado = ESTADOS_TURNO.map((estado) => ({ value: estado, label: estado }));
  // El profesional solo registra si el paciente fue atendido o no se
  // presentó; los estados previos (Pendiente/Confirmado) los administra el
  // Administrador (el backend rechaza cualquier otro valor para este rol).
  const opcionesEstadoProfesional = (['Atendido', 'Cancelado'] as EstadoTurno[]).map((estado) => ({
    value: estado,
    label: estado,
  }));
  // Si el turno todavía está en un estado administrativo (Pendiente/
  // Confirmado), se agrega como primera opción de solo lectura para que el
  // select lo siga mostrando bien — el profesional puede avanzarlo a
  // Atendido/Cancelado, pero no volver a elegir un estado administrativo.
  const opcionesEstadoParaProfesional = (turno: Turno) =>
    opcionesEstadoProfesional.some((opcion) => opcion.value === turno.estado)
      ? opcionesEstadoProfesional
      : [{ value: turno.estado, label: turno.estado }, ...opcionesEstadoProfesional];
  const opcionesHorario = horarios.map((h) => ({ value: h, label: formatearHorario(h) }));

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

      {esProfesional && (
        <Group mb="lg" align="flex-end">
          <NumberInput
            label="Duración de turno (minutos)"
            description={duracionActual !== null ? `Actual: ${duracionActual} min` : undefined}
            min={5}
            max={180}
            step={5}
            w={220}
            value={duracionInput}
            onChange={(valor) => setDuracionInput(typeof valor === 'number' ? valor : '')}
          />
          <Button variant="default" loading={guardandoDuracion} onClick={manejarGuardarDuracion}>
            Guardar duración
          </Button>
        </Group>
      )}

      <Group mb="md" align="flex-end">
        <DateInput
          label="Fecha"
          placeholder="Todas las fechas"
          value={filtroFecha}
          onChange={setFiltroFecha}
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
              {(esAdministrador || esProfesional) && <Table.Th w={esAdministrador ? 100 : 160} />}
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {turnos.length === 0 ? (
              <Table.Tr>
                <Table.Td colSpan={esAdministrador || esProfesional ? 6 : 5}>
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
                  {esProfesional && (
                    <Table.Td>
                      <Select
                        size="xs"
                        w={150}
                        aria-label="Cambiar estado del turno"
                        data={opcionesEstadoParaProfesional(turno)}
                        value={turno.estado}
                        allowDeselect={false}
                        onChange={(valor) => valor && valor !== turno.estado && manejarCambiarEstado(turno, valor as EstadoTurno)}
                      />
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
              onChange={(valor) => {
                form.setFieldValue('profesionalId', valor ?? '');
                form.setFieldValue('horario', null); // la grilla cambia, no queda un horario viejo seleccionado
              }}
            />
            <DateInput
              label="Fecha"
              placeholder="Seleccionar fecha"
              required
              minDate={turnoEnEdicion ? undefined : new Date()}
              {...form.getInputProps('fecha')}
              onChange={(valor) => {
                form.setFieldValue('fecha', valor);
                form.setFieldValue('horario', null);
              }}
            />
            <Select
              label="Horario"
              placeholder={
                !form.values.profesionalId || !form.values.fecha
                  ? 'Elegí profesional y fecha primero'
                  : cargandoHorarios
                    ? 'Buscando horarios...'
                    : opcionesHorario.length === 0
                      ? 'No hay horarios libres ese día'
                      : 'Seleccionar horario'
              }
              required
              disabled={!form.values.profesionalId || !form.values.fecha || opcionesHorario.length === 0}
              data={opcionesHorario}
              {...form.getInputProps('horario')}
            />
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
