import {
  ActionIcon,
  Alert,
  Badge,
  Button,
  Center,
  Group,
  Loader,
  Modal,
  NumberInput,
  PasswordInput,
  Stack,
  Table,
  Text,
  TextInput,
  Title,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import { IconAlertCircle, IconBan, IconEdit, IconPlayerPlay, IconPlus, IconTrash } from '@tabler/icons-react';
import { useEffect, useState } from 'react';
import type { CrearProfesionalDto, Profesional } from '../modelos/profesional';
import { profesionalesServicio } from '../servicios/profesionalesServicio';
import { obtenerErroresDeCampo, obtenerMensajeError } from '../utilidades/manejadorErrores';

// Se usa la forma "completa" (incluye usuario/contraseña) también para
// editar: en ese caso usuario/contraseña-de-alta simplemente no se muestran
// ni se mandan (ver manejarEnvio), pero mantener un solo tipo de formulario
// evita duplicar toda la lógica de validación entre alta y edición.
const VALORES_INICIALES: CrearProfesionalDto = {
  nombre: '',
  apellido: '',
  especialidad: '',
  duracionTurnoMinutos: 30,
  email: '',
  nuevaContrasena: '',
  nombreUsuario: '',
  contrasena: '',
};

export function PaginaProfesionales() {
  const [profesionales, setProfesionales] = useState<Profesional[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modalAbierto, setModalAbierto] = useState(false);
  const [profesionalEnEdicion, setProfesionalEnEdicion] = useState<Profesional | null>(null);
  const [guardando, setGuardando] = useState(false);

  const form = useForm<CrearProfesionalDto>({
    initialValues: VALORES_INICIALES,
    validate: {
      nombre: (valor) =>
        valor.trim().length === 0 ? 'El nombre es obligatorio.' : valor.length > 100 ? 'Máximo 100 caracteres.' : null,
      apellido: (valor) =>
        valor.trim().length === 0 ? 'El apellido es obligatorio.' : valor.length > 100 ? 'Máximo 100 caracteres.' : null,
      especialidad: (valor) =>
        valor.trim().length === 0 ? 'La especialidad es obligatoria.' : valor.length > 100 ? 'Máximo 100 caracteres.' : null,
      duracionTurnoMinutos: (valor) =>
        valor >= 5 && valor <= 180 && valor % 5 === 0 ? null : 'Debe ser un múltiplo de 5, entre 5 y 180 minutos.',
      // El usuario/contraseña de alta y la nueva contraseña de edición se
      // validan a mano en manejarEnvio (son condicionales según el modo).
    },
  });

  const cargarProfesionales = async () => {
    setCargando(true);
    setError(null);
    try {
      const datos = await profesionalesServicio.obtenerTodos();
      setProfesionales(datos);
    } catch (err) {
      setError(obtenerMensajeError(err));
    } finally {
      setCargando(false);
    }
  };

  useEffect(() => {
    void cargarProfesionales();
  }, []);

  const abrirModalCrear = () => {
    setProfesionalEnEdicion(null);
    form.setValues(VALORES_INICIALES);
    form.clearErrors();
    setModalAbierto(true);
  };

  const abrirModalEditar = (profesional: Profesional) => {
    setProfesionalEnEdicion(profesional);
    form.setValues({
      nombre: profesional.nombre,
      apellido: profesional.apellido,
      especialidad: profesional.especialidad,
      duracionTurnoMinutos: profesional.duracionTurnoMinutos,
      email: profesional.email ?? '',
      nuevaContrasena: '',
      nombreUsuario: '',
      contrasena: '',
    });
    form.clearErrors();
    setModalAbierto(true);
  };

  const manejarEnvio = form.onSubmit(async (valores) => {
    if (!profesionalEnEdicion) {
      let hayErroresDeCuenta = false;
      if (!valores.nombreUsuario.trim()) {
        form.setFieldError('nombreUsuario', 'El usuario es obligatorio.');
        hayErroresDeCuenta = true;
      }
      if (valores.contrasena.length < 6) {
        form.setFieldError('contrasena', 'La contraseña debe tener al menos 6 caracteres.');
        hayErroresDeCuenta = true;
      }
      if (hayErroresDeCuenta) return;
    } else if (valores.nuevaContrasena && valores.nuevaContrasena.length < 6) {
      // Dejarla vacía es válido (significa "no cambiarla"); si escribió algo, tiene que ser una contraseña real.
      form.setFieldError('nuevaContrasena', 'La contraseña debe tener al menos 6 caracteres.');
      return;
    }

    setGuardando(true);
    try {
      if (profesionalEnEdicion) {
        const { nombre, apellido, especialidad, duracionTurnoMinutos, email, nuevaContrasena } = valores;
        await profesionalesServicio.actualizar(profesionalEnEdicion.id, {
          nombre,
          apellido,
          especialidad,
          duracionTurnoMinutos,
          email,
          nuevaContrasena: nuevaContrasena?.trim() || undefined,
        });
        notifications.show({ color: 'green', message: 'Profesional actualizado correctamente.' });
      } else {
        await profesionalesServicio.crear(valores);
        notifications.show({ color: 'green', message: 'Profesional creado correctamente.' });
      }
      setModalAbierto(false);
      await cargarProfesionales();
    } catch (err) {
      const erroresDeCampo = obtenerErroresDeCampo(err);
      if (Object.keys(erroresDeCampo).length > 0) {
        form.setErrors(erroresDeCampo);
      } else {
        // Acá cae, entre otros casos, el 409 de nombre de usuario repetido.
        notifications.show({ color: 'red', message: obtenerMensajeError(err) });
      }
    } finally {
      setGuardando(false);
    }
  });

  const manejarDesactivar = (profesional: Profesional) => {
    modals.openConfirmModal({
      title: 'Desactivar profesional',
      children: (
        <Text size="sm">
          ¿Confirmás desactivar a <strong>{profesional.nombre} {profesional.apellido}</strong>? No va a poder
          loguearse ni se lo va a poder elegir para turnos nuevos (sus turnos ya cargados no se tocan). Podés
          reactivarlo cuando quieras.
        </Text>
      ),
      labels: { confirm: 'Desactivar', cancel: 'Volver' },
      confirmProps: { color: 'orange' },
      onConfirm: async () => {
        try {
          await profesionalesServicio.desactivar(profesional.id);
          notifications.show({ color: 'green', message: 'Profesional desactivado correctamente.' });
          await cargarProfesionales();
        } catch (err) {
          notifications.show({ color: 'red', message: obtenerMensajeError(err) });
        }
      },
    });
  };

  const manejarReactivar = async (profesional: Profesional) => {
    try {
      await profesionalesServicio.reactivar(profesional.id);
      notifications.show({ color: 'green', message: 'Profesional reactivado correctamente.' });
      await cargarProfesionales();
    } catch (err) {
      notifications.show({ color: 'red', message: obtenerMensajeError(err) });
    }
  };

  const manejarEliminar = (profesional: Profesional) => {
    modals.openConfirmModal({
      title: 'Eliminar profesional',
      children: (
        <Text size="sm">
          ¿Confirmás eliminar definitivamente a <strong>{profesional.nombre} {profesional.apellido}</strong>? Esta
          acción no se puede deshacer.
        </Text>
      ),
      labels: { confirm: 'Eliminar definitivamente', cancel: 'Volver' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        try {
          await profesionalesServicio.eliminar(profesional.id);
          notifications.show({ color: 'green', message: 'Profesional eliminado correctamente.' });
          await cargarProfesionales();
        } catch (err) {
          notifications.show({ color: 'red', message: obtenerMensajeError(err) });
        }
      },
    });
  };

  return (
    <>
      <Group justify="space-between" mb="lg">
        <Title order={2}>Profesionales</Title>
        <Button leftSection={<IconPlus size={16} />} onClick={abrirModalCrear}>
          Nuevo profesional
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
              <Table.Th>Nombre</Table.Th>
              <Table.Th>Apellido</Table.Th>
              <Table.Th>Especialidad</Table.Th>
              <Table.Th>Email</Table.Th>
              <Table.Th>Duración turno</Table.Th>
              <Table.Th>Estado</Table.Th>
              <Table.Th w={120} />
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {profesionales.length === 0 ? (
              <Table.Tr>
                <Table.Td colSpan={7}>
                  <Text c="dimmed" ta="center" py="md">
                    Todavía no hay profesionales cargados.
                  </Text>
                </Table.Td>
              </Table.Tr>
            ) : (
              profesionales.map((profesional) => (
                <Table.Tr key={profesional.id}>
                  <Table.Td>{profesional.nombre}</Table.Td>
                  <Table.Td>{profesional.apellido}</Table.Td>
                  <Table.Td>{profesional.especialidad}</Table.Td>
                  <Table.Td>{profesional.email ?? '—'}</Table.Td>
                  <Table.Td>{profesional.duracionTurnoMinutos} min</Table.Td>
                  <Table.Td>
                    <Badge color={profesional.activo ? 'green' : 'gray'} variant="light">
                      {profesional.activo ? 'Activo' : 'Inactivo'}
                    </Badge>
                  </Table.Td>
                  <Table.Td>
                    <Group gap={4} wrap="nowrap">
                      <ActionIcon variant="subtle" onClick={() => abrirModalEditar(profesional)} aria-label="Editar profesional">
                        <IconEdit size={16} />
                      </ActionIcon>
                      {profesional.activo ? (
                        <ActionIcon
                          variant="subtle"
                          color="orange"
                          onClick={() => manejarDesactivar(profesional)}
                          aria-label="Desactivar profesional"
                        >
                          <IconBan size={16} />
                        </ActionIcon>
                      ) : (
                        <ActionIcon
                          variant="subtle"
                          color="green"
                          onClick={() => manejarReactivar(profesional)}
                          aria-label="Reactivar profesional"
                        >
                          <IconPlayerPlay size={16} />
                        </ActionIcon>
                      )}
                      {profesional.puedeEliminarse && (
                        <ActionIcon
                          variant="subtle"
                          color="red"
                          onClick={() => manejarEliminar(profesional)}
                          aria-label="Eliminar profesional"
                        >
                          <IconTrash size={16} />
                        </ActionIcon>
                      )}
                    </Group>
                  </Table.Td>
                </Table.Tr>
              ))
            )}
          </Table.Tbody>
        </Table>
      )}

      <Modal
        opened={modalAbierto}
        onClose={() => setModalAbierto(false)}
        title={profesionalEnEdicion ? 'Editar profesional' : 'Nuevo profesional'}
      >
        <form onSubmit={manejarEnvio}>
          <Stack>
            <TextInput label="Nombre" required maxLength={100} {...form.getInputProps('nombre')} />
            <TextInput label="Apellido" required maxLength={100} {...form.getInputProps('apellido')} />
            <TextInput
              label="Especialidad"
              required
              maxLength={100}
              placeholder="Clínica Médica"
              {...form.getInputProps('especialidad')}
            />
            <TextInput
              label="Email"
              type="email"
              maxLength={150}
              placeholder="laura.gomez@clinica.com"
              {...form.getInputProps('email')}
            />
            <NumberInput
              label="Duración de cada turno (minutos)"
              description="Define cada cuánto se ofrecen horarios al pedir un turno con este profesional."
              required
              min={5}
              max={180}
              step={5}
              {...form.getInputProps('duracionTurnoMinutos')}
            />
            {!profesionalEnEdicion && (
              <>
                <Text size="sm" c="dimmed" mt="xs">
                  Con estos datos el profesional va a poder loguearse y ver su propia agenda.
                </Text>
                <TextInput
                  label="Usuario"
                  required
                  maxLength={50}
                  placeholder="laura.gomez"
                  {...form.getInputProps('nombreUsuario')}
                />
                <PasswordInput label="Contraseña" required {...form.getInputProps('contrasena')} />
              </>
            )}
            {profesionalEnEdicion && (
              <PasswordInput
                label="Nueva contraseña"
                description="Dejar en blanco para no cambiarla."
                placeholder="••••••••"
                {...form.getInputProps('nuevaContrasena')}
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
