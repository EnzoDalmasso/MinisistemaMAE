import { Anchor, Button, Card, Container, PasswordInput, Stack, Text, TextInput, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAutenticacion } from '../hooks/useAutenticacion';
import { obtenerMensajeError } from '../utilidades/manejadorErrores';

export function PaginaLogin() {
  const { iniciarSesion, cargando } = useAutenticacion();
  const navegar = useNavigate();
  const [errorGeneral, setErrorGeneral] = useState<string | null>(null);

  const form = useForm({
    initialValues: { nombreUsuario: '', contrasena: '' },
    validate: {
      nombreUsuario: (valor) => (valor.trim().length === 0 ? 'El usuario es obligatorio.' : null),
      contrasena: (valor) => (valor.length === 0 ? 'La contraseña es obligatoria.' : null),
    },
  });

  const manejarEnvio = form.onSubmit(async (valores) => {
    setErrorGeneral(null);
    try {
      await iniciarSesion(valores.nombreUsuario, valores.contrasena);
      navegar('/', { replace: true });
    } catch (error) {
      setErrorGeneral(obtenerMensajeError(error));
    }
  });

  return (
    <Container size={420} my={80}>
      <Title ta="center" order={2}>
        Clínica
      </Title>
      <Text c="dimmed" size="sm" ta="center" mt={5}>
        Sistema de gestión de turnos
      </Text>

      <Card withBorder shadow="sm" padding="lg" radius="md" mt="xl">
        <form onSubmit={manejarEnvio}>
          <Stack>
            <TextInput
              label="Usuario"
              placeholder="administrador"
              required
              autoFocus
              {...form.getInputProps('nombreUsuario')}
            />
            <PasswordInput
              label="Contraseña"
              placeholder="Tu contraseña"
              required
              {...form.getInputProps('contrasena')}
            />
            {errorGeneral && (
              <Text c="red" size="sm" role="alert">
                {errorGeneral}
              </Text>
            )}
            <Button type="submit" fullWidth loading={cargando}>
              Iniciar sesión
            </Button>
          </Stack>
        </form>
      </Card>

      <Text ta="center" size="sm" mt="md">
        ¿Venís a sacar un turno? <Anchor component={Link} to="/acceso-paciente">Entrá acá</Anchor>
      </Text>
    </Container>
  );
}
