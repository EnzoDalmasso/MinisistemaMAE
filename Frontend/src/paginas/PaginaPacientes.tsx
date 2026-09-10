import { ActionIcon, Alert, Button, Center, Group, Loader, Modal, Stack, Table, Text, TextInput, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { IconAlertCircle, IconEdit, IconPlus } from '@tabler/icons-react';
import { useEffect, useState } from 'react';
import type { GuardarPacienteDto, Paciente } from '../modelos/paciente';
import { pacientesServicio } from '../servicios/pacientesServicio';
import { obtenerErroresDeCampo, obtenerMensajeError } from '../utilidades/manejadorErrores';

const VALORES_INICIALES: GuardarPacienteDto = { nombre: '', apellido: '', telefono: '', obraSocial: '' };

export function PaginaPacientes() {
  const [pacientes, setPacientes] = useState<Paciente[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modalAbierto, setModalAbierto] = useState(false);
  const [pacienteEnEdicion, setPacienteEnEdicion] = useState<Paciente | null>(null);
  const [guardando, setGuardando] = useState(false);

  const form = useForm<GuardarPacienteDto>({
    initialValues: VALORES_INICIALES,
    validate: {
      nombre: (valor) =>
        valor.trim().length === 0 ? 'El nombre es obligatorio.' : valor.length > 100 ? 'Máximo 100 caracteres.' : null,
      apellido: (valor) =>
        valor.trim().length === 0 ? 'El apellido es obligatorio.' : valor.length > 100 ? 'Máximo 100 caracteres.' : null,
      telefono: (valor) => {
        if (valor.trim().length === 0) return 'El teléfono es obligatorio.';
        if (valor.length > 30) return 'Máximo 30 caracteres.';
        if (!/^[0-9+()\-\s]+$/.test(valor)) return 'El teléfono contiene caracteres inválidos.';
        return null;
      },
      obraSocial: (valor) =>
        valor.trim().length === 0 ? 'La obra social es obligatoria.' : valor.length > 100 ? 'Máximo 100 caracteres.' : null,
    },
  });

  const cargarPacientes = async () => {
    setCargando(true);
    setError(null);
    try {
      const datos = await pacientesServicio.obtenerTodos();
      setPacientes(datos);
    } catch (err) {
      setError(obtenerMensajeError(err));
    } finally {
      setCargando(false);
    }
  };

  useEffect(() => {
    void cargarPacientes();
  }, []);

  const abrirModalCrear = () => {
    setPacienteEnEdicion(null);
    form.setValues(VALORES_INICIALES);
    form.clearErrors();
    setModalAbierto(true);
  };

  const abrirModalEditar = (paciente: Paciente) => {
    setPacienteEnEdicion(paciente);
    form.setValues({
      nombre: paciente.nombre,
      apellido: paciente.apellido,
      telefono: paciente.telefono,
      obraSocial: paciente.obraSocial,
    });
    form.clearErrors();
    setModalAbierto(true);
  };

  const manejarEnvio = form.onSubmit(async (valores) => {
    setGuardando(true);
    try {
      if (pacienteEnEdicion) {
        await pacientesServicio.actualizar(pacienteEnEdicion.id, valores);
        notifications.show({ color: 'green', message: 'Paciente actualizado correctamente.' });
      } else {
        await pacientesServicio.crear(valores);
        notifications.show({ color: 'green', message: 'Paciente creado correctamente.' });
      }
      setModalAbierto(false);
      await cargarPacientes();
    } catch (err) {
      const erroresDeCampo = obtenerErroresDeCampo(err);
      if (Object.keys(erroresDeCampo).length > 0) {
        form.setErrors(erroresDeCampo);
      } else {
        notifications.show({ color: 'red', message: obtenerMensajeError(err) });
      }
    } finally {
      setGuardando(false);
    }
  });

  return (
    <>
      <Group justify="space-between" mb="lg">
        <Title order={2}>Pacientes</Title>
        <Button leftSection={<IconPlus size={16} />} onClick={abrirModalCrear}>
          Nuevo paciente
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
              <Table.Th>Teléfono</Table.Th>
              <Table.Th>Obra social</Table.Th>
              <Table.Th w={80} />
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {pacientes.length === 0 ? (
              <Table.Tr>
                <Table.Td colSpan={5}>
                  <Text c="dimmed" ta="center" py="md">
                    Todavía no hay pacientes cargados.
                  </Text>
                </Table.Td>
              </Table.Tr>
            ) : (
              pacientes.map((paciente) => (
                <Table.Tr key={paciente.id}>
                  <Table.Td>{paciente.nombre}</Table.Td>
                  <Table.Td>{paciente.apellido}</Table.Td>
                  <Table.Td>{paciente.telefono}</Table.Td>
                  <Table.Td>{paciente.obraSocial}</Table.Td>
                  <Table.Td>
                    <ActionIcon variant="subtle" onClick={() => abrirModalEditar(paciente)} aria-label="Editar paciente">
                      <IconEdit size={16} />
                    </ActionIcon>
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
        title={pacienteEnEdicion ? 'Editar paciente' : 'Nuevo paciente'}
      >
        <form onSubmit={manejarEnvio}>
          <Stack>
            <TextInput label="Nombre" required maxLength={100} {...form.getInputProps('nombre')} />
            <TextInput label="Apellido" required maxLength={100} {...form.getInputProps('apellido')} />
            <TextInput
              label="Teléfono"
              required
              maxLength={30}
              placeholder="11-5555-0000"
              {...form.getInputProps('telefono')}
            />
            <TextInput label="Obra social" required maxLength={100} {...form.getInputProps('obraSocial')} />
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
