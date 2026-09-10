import { ActionIcon, Alert, Button, Center, Group, Loader, Modal, Stack, Table, Text, TextInput, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { IconAlertCircle, IconEdit, IconPlus } from '@tabler/icons-react';
import { useEffect, useState } from 'react';
import type { GuardarProfesionalDto, Profesional } from '../modelos/profesional';
import { profesionalesServicio } from '../servicios/profesionalesServicio';
import { obtenerErroresDeCampo, obtenerMensajeError } from '../utilidades/manejadorErrores';

const VALORES_INICIALES: GuardarProfesionalDto = { nombre: '', apellido: '', especialidad: '' };

export function PaginaProfesionales() {
  const [profesionales, setProfesionales] = useState<Profesional[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modalAbierto, setModalAbierto] = useState(false);
  const [profesionalEnEdicion, setProfesionalEnEdicion] = useState<Profesional | null>(null);
  const [guardando, setGuardando] = useState(false);

  const form = useForm<GuardarProfesionalDto>({
    initialValues: VALORES_INICIALES,
    validate: {
      nombre: (valor) =>
        valor.trim().length === 0 ? 'El nombre es obligatorio.' : valor.length > 100 ? 'Máximo 100 caracteres.' : null,
      apellido: (valor) =>
        valor.trim().length === 0 ? 'El apellido es obligatorio.' : valor.length > 100 ? 'Máximo 100 caracteres.' : null,
      especialidad: (valor) =>
        valor.trim().length === 0 ? 'La especialidad es obligatoria.' : valor.length > 100 ? 'Máximo 100 caracteres.' : null,
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
    form.setValues({ nombre: profesional.nombre, apellido: profesional.apellido, especialidad: profesional.especialidad });
    form.clearErrors();
    setModalAbierto(true);
  };

  const manejarEnvio = form.onSubmit(async (valores) => {
    setGuardando(true);
    try {
      if (profesionalEnEdicion) {
        await profesionalesServicio.actualizar(profesionalEnEdicion.id, valores);
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
        notifications.show({ color: 'red', message: obtenerMensajeError(err) });
      }
    } finally {
      setGuardando(false);
    }
  });

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
              <Table.Th w={80} />
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {profesionales.length === 0 ? (
              <Table.Tr>
                <Table.Td colSpan={4}>
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
                  <Table.Td>
                    <ActionIcon variant="subtle" onClick={() => abrirModalEditar(profesional)} aria-label="Editar profesional">
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
