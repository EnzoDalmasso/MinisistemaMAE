import { Group, Paper, Text, ThemeIcon } from '@mantine/core';
import type { ComponentType } from 'react';

interface Props {
  titulo: string;
  valor: number;
  color: string;
  icono: ComponentType<{ size?: number | string }>;
}

export function TarjetaEstadistica({ titulo, valor, color, icono: Icono }: Props) {
  return (
    <Paper withBorder radius="md" p="md">
      <Group justify="space-between" align="flex-start">
        <div>
          <Text c="dimmed" size="sm" fw={500}>
            {titulo}
          </Text>
          <Text fw={700} size="xl">
            {valor}
          </Text>
        </div>
        <ThemeIcon color={color} variant="light" size={38} radius="md">
          <Icono size={22} />
        </ThemeIcon>
      </Group>
    </Paper>
  );
}
