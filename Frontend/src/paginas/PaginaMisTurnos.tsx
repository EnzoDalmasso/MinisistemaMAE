import { ActionIcon, Alert, Button, Center, Group, Loader, Modal, Select, Stack, Table, Text, TextInput, Title } from '@mantine/core';
import { DateInput } from '@mantine/dates';
import { useForm } from '@mantine/form';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import { IconAlertCircle, IconCalendarOff, IconClockEdit, IconPlus } from '@tabler/icons-react';
import { useEffect, useState } from 'react';
import { EstadoTurnoBadge } from '../componentes/EstadoTurnoBadge';
import { useAutenticacion } from '../hooks/useAutenticacion';
import { useHorariosDisponibles } from '../hooks/useHorariosDisponibles';
import type { Paciente } from '../modelos/paciente';
import type { Profesional } from '../modelos/profesional';
import type { Turno } from '../modelos/turno';
import { pacientesServicio } from '../servicios/pacientesServicio';
import { profesionalesServicio } from '../servicios/profesionalesServicio';
import { turnosServicio } from '../servicios/turnosServicio';
import { formatearFecha, formatearHorario } from '../utilidades/formato';
import { obtenerErroresDeCampo, obtenerMensajeError } from '../utilidades/manejadorErrores';

interface ValoresNuevoTurno {
  profesionalId: string;
  fecha: string | null;
  horario: string | null;
  // Solo se piden (y se validan) cuando el paciente todavía no los tiene
  // cargados — ver "datosDeContactoCompletos".
  telefono: string;
  obraSocial: string;
  email: string;
}

const VALORES_INICIALES: ValoresNuevoTurno = {
  profesionalId: '',
  fecha: null,
  horario: null,
  telefono: '',
  obraSocial: '',
  email: '',
};

interface ValoresReprogramar {
  fecha: string | null;
  horario: string | null;
}

// Un turno solo se puede reprogramar o cancelar mientras sigue "activo": uno
// ya cancelado o atendido es historial, no tiene sentido tocarlo.
const ESTADOS_MODIFICABLES = new Set(['Pendiente', 'Confirmado']);

export function PaginaMisTurnos() {
  const { sesion } = useAutenticacion();

  const [turnos, setTurnos] = useState<Turno[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [profesionales, setProfesionales] = useState<Profesional[]>([]);
  const [miPerfil, setMiPerfil] = useState<Paciente | null>(null);

  const [modalNuevoAbierto, setModalNuevoAbierto] = useState(false);
  const [guardandoNuevo, setGuardandoNuevo] = useState(false);

  const [turnoAReprogramar, setTurnoAReprogramar] = useState<Turno | null>(null);
  const [reprogramando, setReprogramando] = useState(false);

  const formNuevo = useForm<ValoresNuevoTurno>({
    initialValues: VALORES_INICIALES,
    validate: {
      profesionalId: (valor) => (valor ? null : 'Debe seleccionar un profesional.'),
      fecha: (valor) => (valor ? null : 'La fecha es obligatoria.'),
      horario: (valor) => (valor ? null : 'El horario es obligatorio.'),
    },
  });

  const formReprogramar = useForm<ValoresReprogramar>({
    initialValues: { fecha: null, horario: null },
    validate: {
      fecha: (valor) => (valor ? null : 'La fecha es obligatoria.'),
      horario: (valor) => (valor ? null : 'El horario es obligatorio.'),
    },
  });

  // La grilla de horarios se recalcula sola apenas hay profesional + fecha.
  const { horarios: horariosNuevo, cargando: cargandoHorariosNuevo } = useHorariosDisponibles(
    formNuevo.values.profesionalId,
    formNuevo.values.fecha,
  );
  const { horarios: horariosReprogramar, cargando: cargandoHorariosReprogramar } = useHorariosDisponibles(
    turnoAReprogramar?.profesionalId,
    formReprogramar.values.fecha,
    turnoAReprogramar?.id,
  );

  const cargarTurnos = async () => {
    setCargando(true);
    setError(null);
    try {
      // El backend ya filtra por el paciente de la sesión; no hace falta (ni
      // se puede) pedirle turnos de otro paciente desde acá.
      const datos = await turnosServicio.obtener({});
      setTurnos(datos);
    } catch (err) {
      setError(obtenerMensajeError(err));
    } finally {
      setCargando(false);
    }
  };

  useEffect(() => {
    void cargarTurnos();
    profesionalesServicio.obtenerTodos().then(setProfesionales).catch(() => undefined);
    pacientesServicio.obtenerMiPerfil().then(setMiPerfil).catch(() => undefined);
  }, []);

  // Un paciente autogestionado arranca sin estos datos: se le piden recién
  // al pedir su primer turno, y una vez completos no se le vuelven a pedir.
  const datosDeContactoCompletos = Boolean(miPerfil?.telefono && miPerfil?.obraSocial && miPerfil?.email);

  const abrirModalNuevo = () => {
    formNuevo.setValues({
      ...VALORES_INICIALES,
      telefono: miPerfil?.telefono ?? '',
      obraSocial: miPerfil?.obraSocial ?? '',
      email: miPerfil?.email ?? '',
    });
    formNuevo.clearErrors();
    setModalNuevoAbierto(true);
  };

  // Reserva el turno (y, si hace falta, guarda antes los datos de contacto).
  // Se llama tanto desde el submit manual (primer turno, con los campos de
  // contacto) como automáticamente al elegir un horario (turnos siguientes).
  const confirmarNuevoTurno = async (valores: ValoresNuevoTurno) => {
    if (!valores.fecha || !valores.horario) return;

    setGuardandoNuevo(true);
    try {
      if (!datosDeContactoCompletos) {
        const perfilActualizado = await pacientesServicio.actualizarMiContacto({
          telefono: valores.telefono.trim(),
          obraSocial: valores.obraSocial.trim(),
          email: valores.email.trim(),
        });
        setMiPerfil(perfilActualizado);
      }

      await turnosServicio.crear({
        pacienteId: sesion?.pacienteId ?? 0,
        profesionalId: Number(valores.profesionalId),
        fecha: valores.fecha,
        horario: valores.horario,
      });
      notifications.show({ color: 'green', message: 'Turno solicitado correctamente.' });
      setModalNuevoAbierto(false);
      await cargarTurnos();
    } catch (err) {
      const erroresDeCampo = obtenerErroresDeCampo(err);
      if (Object.keys(erroresDeCampo).length > 0) {
        formNuevo.setErrors(erroresDeCampo);
      } else {
        // Acá cae, entre otros casos, el 409 de la regla de disponibilidad
        // (alguien tomó ese horario mientras se elegía): el modal queda
        // abierto para que se pueda probar otro horario de la grilla.
        notifications.show({ color: 'red', message: obtenerMensajeError(err) });
      }
    } finally {
      setGuardandoNuevo(false);
    }
  };

  // Solo se usa cuando todavía hay que pedir datos de contacto (primer
  // turno): en ese caso queda un botón "Confirmar" explícito porque hay más
  // campos para completar después de elegir el horario.
  const manejarEnvioNuevo = formNuevo.onSubmit(async (valores) => {
    if (!valores.fecha || !valores.horario) {
      formNuevo.validate();
      return;
    }

    let hayErroresDeContacto = false;
    if (!valores.telefono.trim()) {
      formNuevo.setFieldError('telefono', 'El teléfono es obligatorio.');
      hayErroresDeContacto = true;
    }
    if (!valores.obraSocial.trim()) {
      formNuevo.setFieldError('obraSocial', 'La obra social es obligatoria.');
      hayErroresDeContacto = true;
    }
    if (!valores.email.trim()) {
      formNuevo.setFieldError('email', 'El email es obligatorio.');
      hayErroresDeContacto = true;
    }
    if (hayErroresDeContacto) return;

    await confirmarNuevoTurno(valores);
  });

  const abrirModalReprogramar = (turno: Turno) => {
    setTurnoAReprogramar(turno);
    formReprogramar.setValues({ fecha: turno.fecha, horario: turno.horario });
    formReprogramar.clearErrors();
  };

  const manejarEnvioReprogramar = formReprogramar.onSubmit(async (valores) => {
    if (!turnoAReprogramar || !valores.fecha || !valores.horario) {
      formReprogramar.validate();
      return;
    }

    setReprogramando(true);
    try {
      await turnosServicio.reprogramar(turnoAReprogramar.id, { fecha: valores.fecha, horario: valores.horario });
      notifications.show({ color: 'green', message: 'Turno reprogramado correctamente.' });
      setTurnoAReprogramar(null);
      await cargarTurnos();
    } catch (err) {
      const erroresDeCampo = obtenerErroresDeCampo(err);
      if (Object.keys(erroresDeCampo).length > 0) {
        formReprogramar.setErrors(erroresDeCampo);
      } else {
        notifications.show({ color: 'red', message: obtenerMensajeError(err) });
      }
    } finally {
      setReprogramando(false);
    }
  });

  const manejarCancelar = (turno: Turno) => {
    modals.openConfirmModal({
      title: 'Cancelar turno',
      children: (
        <Text size="sm">
          ¿Confirmás cancelar tu turno con <strong>{turno.profesionalNombreCompleto}</strong> el{' '}
          {formatearFecha(turno.fecha)} a las {formatearHorario(turno.horario)}? Esta acción no se puede deshacer.
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

  const opcionesProfesionales = profesionales.map((p) => ({
    value: String(p.id),
    label: `${p.nombre} ${p.apellido} — ${p.especialidad}`,
  }));

  const aOpcionesHorario = (horarios: string[]) => horarios.map((h) => ({ value: h, label: formatearHorario(h) }));

  return (
    <>
      <Group justify="space-between" mb="lg">
        <Title order={2}>Mis turnos</Title>
        <Button leftSection={<IconPlus size={16} />} onClick={abrirModalNuevo}>
          Pedir turno
        </Button>
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
              <Table.Th>Profesional</Table.Th>
              <Table.Th>Fecha</Table.Th>
              <Table.Th>Horario</Table.Th>
              <Table.Th>Estado</Table.Th>
              <Table.Th w={100} />
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {turnos.length === 0 ? (
              <Table.Tr>
                <Table.Td colSpan={5}>
                  <Text c="dimmed" ta="center" py="md">
                    Todavía no pediste ningún turno.
                  </Text>
                </Table.Td>
              </Table.Tr>
            ) : (
              turnos.map((turno) => (
                <Table.Tr key={turno.id}>
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
                  <Table.Td>
                    {ESTADOS_MODIFICABLES.has(turno.estado) && (
                      <Group gap={4} wrap="nowrap">
                        <ActionIcon variant="subtle" onClick={() => abrirModalReprogramar(turno)} aria-label="Reprogramar turno">
                          <IconClockEdit size={16} />
                        </ActionIcon>
                        <ActionIcon variant="subtle" color="red" onClick={() => manejarCancelar(turno)} aria-label="Cancelar turno">
                          <IconCalendarOff size={16} />
                        </ActionIcon>
                      </Group>
                    )}
                  </Table.Td>
                </Table.Tr>
              ))
            )}
          </Table.Tbody>
        </Table>
      )}

      <Modal opened={modalNuevoAbierto} onClose={() => setModalNuevoAbierto(false)} title="Pedir turno">
        <form onSubmit={manejarEnvioNuevo}>
          <Stack>
            <Select
              label="Profesional"
              placeholder="Seleccionar profesional"
              required
              searchable
              data={opcionesProfesionales}
              {...formNuevo.getInputProps('profesionalId')}
              onChange={(valor) => {
                formNuevo.setFieldValue('profesionalId', valor ?? '');
                formNuevo.setFieldValue('horario', null); // la grilla cambia, no queda un horario viejo seleccionado
              }}
            />
            <DateInput
              label="Fecha"
              placeholder="Seleccionar fecha"
              required
              minDate={new Date()}
              {...formNuevo.getInputProps('fecha')}
              onChange={(valor) => {
                formNuevo.setFieldValue('fecha', valor);
                formNuevo.setFieldValue('horario', null);
              }}
            />
            <Select
              label="Horario"
              placeholder={
                !formNuevo.values.profesionalId || !formNuevo.values.fecha
                  ? 'Elegí profesional y fecha primero'
                  : cargandoHorariosNuevo
                    ? 'Buscando horarios...'
                    : horariosNuevo.length === 0
                      ? 'No hay horarios libres ese día'
                      : datosDeContactoCompletos
                        ? 'Seleccionar horario (confirma el turno al instante)'
                        : 'Seleccionar horario'
              }
              required
              disabled={
                !formNuevo.values.profesionalId || !formNuevo.values.fecha || horariosNuevo.length === 0 || guardandoNuevo
              }
              data={aOpcionesHorario(horariosNuevo)}
              {...formNuevo.getInputProps('horario')}
              onChange={(valor) => {
                formNuevo.setFieldValue('horario', valor);
                // Con los datos de contacto ya cargados, no hace falta un
                // paso extra de confirmación: elegir el horario reserva el
                // turno directamente (mismo horario que se acaba de tipear,
                // no el del estado del formulario, que todavía no se actualizó).
                if (valor && datosDeContactoCompletos) {
                  void confirmarNuevoTurno({ ...formNuevo.values, horario: valor });
                }
              }}
            />
            {datosDeContactoCompletos && guardandoNuevo && (
              <Text size="sm" c="dimmed">
                Reservando turno...
              </Text>
            )}
            {!datosDeContactoCompletos && (
              <>
                <Text size="sm" c="dimmed" mt="xs">
                  Para tu primer turno necesitamos algunos datos de contacto.
                </Text>
                <TextInput
                  label="Teléfono"
                  required
                  placeholder="11-5555-0000"
                  {...formNuevo.getInputProps('telefono')}
                />
                <TextInput label="Obra social" required {...formNuevo.getInputProps('obraSocial')} />
                <TextInput label="Email" type="email" required {...formNuevo.getInputProps('email')} />
              </>
            )}
            <Group justify="flex-end" mt="sm">
              <Button variant="default" onClick={() => setModalNuevoAbierto(false)}>
                {datosDeContactoCompletos ? 'Cerrar' : 'Cancelar'}
              </Button>
              {!datosDeContactoCompletos && (
                <Button type="submit" loading={guardandoNuevo}>
                  Confirmar
                </Button>
              )}
            </Group>
          </Stack>
        </form>
      </Modal>

      <Modal opened={turnoAReprogramar !== null} onClose={() => setTurnoAReprogramar(null)} title="Reprogramar turno">
        <form onSubmit={manejarEnvioReprogramar}>
          <Stack>
            <Text size="sm" c="dimmed">
              Con {turnoAReprogramar?.profesionalNombreCompleto}
            </Text>
            <DateInput
              label="Nueva fecha"
              placeholder="Seleccionar fecha"
              required
              minDate={new Date()}
              {...formReprogramar.getInputProps('fecha')}
              onChange={(valor) => {
                formReprogramar.setFieldValue('fecha', valor);
                formReprogramar.setFieldValue('horario', null);
              }}
            />
            <Select
              label="Nuevo horario"
              placeholder={cargandoHorariosReprogramar ? 'Buscando horarios...' : 'Seleccionar horario'}
              required
              disabled={horariosReprogramar.length === 0}
              data={aOpcionesHorario(horariosReprogramar)}
              {...formReprogramar.getInputProps('horario')}
            />
            <Group justify="flex-end" mt="sm">
              <Button variant="default" onClick={() => setTurnoAReprogramar(null)}>
                Cancelar
              </Button>
              <Button type="submit" loading={reprogramando}>
                Guardar
              </Button>
            </Group>
          </Stack>
        </form>
      </Modal>
    </>
  );
}
