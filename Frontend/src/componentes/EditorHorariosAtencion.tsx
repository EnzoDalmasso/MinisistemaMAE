import { ActionIcon, Button, Checkbox, Group, Modal, Paper, Stack, Text } from '@mantine/core';
import { TimeInput } from '@mantine/dates';
import { notifications } from '@mantine/notifications';
import { IconClock, IconPlus, IconTrash } from '@tabler/icons-react';
import { useState } from 'react';
import { DIAS_SEMANA, type BloqueHorario, type DiaSemana } from '../modelos/profesional';
import { profesionalesServicio } from '../servicios/profesionalesServicio';
import { obtenerMensajeError } from '../utilidades/manejadorErrores';

// Un rango tal como se edita en el formulario: horas en "HH:mm" (lo que
// entrega/espera el TimeInput de Mantine), a diferencia de BloqueHorario que
// las tiene en "HH:mm:ss" (formato de TimeOnly, lo que espera el backend).
interface RangoEditable {
  horaInicio: string;
  horaFin: string;
}

type EstadoDias = Record<DiaSemana, RangoEditable[]>;

function estadoVacio(): EstadoDias {
  return Object.fromEntries(DIAS_SEMANA.map((dia) => [dia.valor, [] as RangoEditable[]])) as unknown as EstadoDias;
}

function estadoDesdeHorarios(horarios: BloqueHorario[]): EstadoDias {
  const estado = estadoVacio();
  for (const bloque of horarios) {
    estado[bloque.diaSemana].push({ horaInicio: bloque.horaInicio.slice(0, 5), horaFin: bloque.horaFin.slice(0, 5) });
  }
  for (const dia of DIAS_SEMANA) {
    estado[dia.valor].sort((a, b) => a.horaInicio.localeCompare(b.horaInicio));
  }
  return estado;
}

// Validación en el cliente para los casos obvios (evita un viaje al backend
// por un error de tipeo); el backend igual revalida todo esto por su cuenta.
function validar(estado: EstadoDias): string | null {
  for (const dia of DIAS_SEMANA) {
    const rangos = estado[dia.valor];
    for (const rango of rangos) {
      if (!rango.horaInicio || !rango.horaFin) {
        return `Completá los horarios del ${dia.etiqueta.toLowerCase()}.`;
      }
      if (rango.horaInicio >= rango.horaFin) {
        return `En ${dia.etiqueta.toLowerCase()}, el horario de inicio debe ser anterior al de fin.`;
      }
    }

    const ordenados = [...rangos].sort((a, b) => a.horaInicio.localeCompare(b.horaInicio));
    for (let i = 1; i < ordenados.length; i++) {
      if (ordenados[i].horaInicio < ordenados[i - 1].horaFin) {
        return `En ${dia.etiqueta.toLowerCase()} hay horarios superpuestos.`;
      }
    }
  }
  return null;
}

interface Props {
  profesionalId: number;
  horarios: BloqueHorario[];
  onGuardado: (horarios: BloqueHorario[]) => void;
  // Por defecto se muestra un botón "Editar horarios de atención"; se puede
  // reemplazar (ej. por un ActionIcon en una fila de tabla) pasando esto.
  renderTrigger?: (abrir: () => void) => React.ReactNode;
}

export function EditorHorariosAtencion({ profesionalId, horarios, onGuardado, renderTrigger }: Props) {
  const [modalAbierto, setModalAbierto] = useState(false);
  const [estado, setEstado] = useState<EstadoDias>(estadoVacio());
  const [guardando, setGuardando] = useState(false);

  const abrir = () => {
    setEstado(estadoDesdeHorarios(horarios));
    setModalAbierto(true);
  };

  const alternarDia = (dia: DiaSemana, activo: boolean) => {
    setEstado((prev) => ({ ...prev, [dia]: activo ? [{ horaInicio: '08:00', horaFin: '12:00' }] : [] }));
  };

  const agregarRango = (dia: DiaSemana) => {
    setEstado((prev) => ({ ...prev, [dia]: [...prev[dia], { horaInicio: '14:00', horaFin: '18:00' }] }));
  };

  const quitarRango = (dia: DiaSemana, indice: number) => {
    setEstado((prev) => ({ ...prev, [dia]: prev[dia].filter((_, i) => i !== indice) }));
  };

  const actualizarRango = (dia: DiaSemana, indice: number, campo: keyof RangoEditable, valor: string) => {
    setEstado((prev) => ({
      ...prev,
      [dia]: prev[dia].map((rango, i) => (i === indice ? { ...rango, [campo]: valor } : rango)),
    }));
  };

  const manejarGuardar = async () => {
    const error = validar(estado);
    if (error) {
      notifications.show({ color: 'red', message: error });
      return;
    }

    const bloques: BloqueHorario[] = DIAS_SEMANA.flatMap((dia) =>
      estado[dia.valor].map((rango) => ({
        diaSemana: dia.valor,
        horaInicio: `${rango.horaInicio}:00`,
        horaFin: `${rango.horaFin}:00`,
      })),
    );

    setGuardando(true);
    try {
      const actualizado = await profesionalesServicio.actualizarHorarios(profesionalId, bloques);
      onGuardado(actualizado.horarios);
      notifications.show({ color: 'green', message: 'Horarios de atención actualizados correctamente.' });
      setModalAbierto(false);
    } catch (err) {
      notifications.show({ color: 'red', message: obtenerMensajeError(err) });
    } finally {
      setGuardando(false);
    }
  };

  return (
    <>
      {renderTrigger ? (
        renderTrigger(abrir)
      ) : (
        <Button variant="default" leftSection={<IconClock size={16} />} onClick={abrir}>
          Editar horarios de atención
        </Button>
      )}

      <Modal opened={modalAbierto} onClose={() => setModalAbierto(false)} title="Horarios de atención" size="lg">
        <Stack>
          <Text size="sm" c="dimmed">
            Elegí los días que atiende y, para cada uno, el o los rangos horarios (por ejemplo, de 8 a 12 y de 14 a
            18 si hay corte al mediodía).
          </Text>

          {DIAS_SEMANA.map((dia) => {
            const rangos = estado[dia.valor];
            const activo = rangos.length > 0;
            return (
              <Paper key={dia.valor} withBorder radius="md" p="sm">
                <Group justify="space-between" mb={activo ? 'xs' : 0}>
                  <Checkbox
                    label={dia.etiqueta}
                    checked={activo}
                    onChange={(evento) => alternarDia(dia.valor, evento.currentTarget.checked)}
                  />
                  {activo && (
                    <ActionIcon
                      variant="subtle"
                      onClick={() => agregarRango(dia.valor)}
                      aria-label={`Agregar otro rango horario el ${dia.etiqueta}`}
                    >
                      <IconPlus size={16} />
                    </ActionIcon>
                  )}
                </Group>

                {activo && (
                  <Stack gap="xs">
                    {rangos.map((rango, indice) => (
                      <Group key={indice} wrap="nowrap">
                        <TimeInput
                          aria-label={`Desde (${dia.etiqueta})`}
                          value={rango.horaInicio}
                          onChange={(evento) => actualizarRango(dia.valor, indice, 'horaInicio', evento.currentTarget.value)}
                          w={110}
                        />
                        <Text size="sm" c="dimmed">
                          a
                        </Text>
                        <TimeInput
                          aria-label={`Hasta (${dia.etiqueta})`}
                          value={rango.horaFin}
                          onChange={(evento) => actualizarRango(dia.valor, indice, 'horaFin', evento.currentTarget.value)}
                          w={110}
                        />
                        <ActionIcon
                          variant="subtle"
                          color="red"
                          onClick={() => quitarRango(dia.valor, indice)}
                          aria-label={`Quitar rango horario el ${dia.etiqueta}`}
                        >
                          <IconTrash size={16} />
                        </ActionIcon>
                      </Group>
                    ))}
                  </Stack>
                )}
              </Paper>
            );
          })}

          <Group justify="flex-end" mt="sm">
            <Button variant="default" onClick={() => setModalAbierto(false)}>
              Cancelar
            </Button>
            <Button onClick={manejarGuardar} loading={guardando}>
              Guardar
            </Button>
          </Group>
        </Stack>
      </Modal>
    </>
  );
}
