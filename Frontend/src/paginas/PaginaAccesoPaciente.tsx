import { Anchor, Button, Card, Container, Stack, Text, TextInput, Title } from '@mantine/core';
import { useForm } from '@mantine/form';
import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAutenticacion } from '../hooks/useAutenticacion';
import { obtenerMensajeError } from '../utilidades/manejadorErrores';

interface ValoresAcceso {
  nombre: string;
  apellido: string;
  dni: string;
}

// A propósito no pide contraseña: es una simplificación deliberada de esta
// demo (decisión explícita del cliente, documentada en el README) — el DNI
// funciona como identificador único. Si ya existe una cuenta con ese DNI,
// el backend ingresa a ella; si no, la crea en el momento.
export function PaginaAccesoPaciente() {
  const { accederComoPaciente, cargando } = useAutenticacion();
  const navegar = useNavigate();
  const [errorGeneral, setErrorGeneral] = useState<string | null>(null);

  const form = useForm<ValoresAcceso>({
    initialValues: { nombre: '', apellido: '', dni: '' },
    validate: {
      nombre: (valor) => (valor.trim().length === 0 ? 'El nombre es obligatorio.' : null),
      apellido: (valor) => (valor.trim().length === 0 ? 'El apellido es obligatorio.' : null),
      dni: (valor) => (/^\d{7,8}$/.test(valor.trim()) ? null : 'Ingresá un DNI válido (7 u 8 dígitos, sin puntos).'),
    },
  });

  const manejarEnvio = form.onSubmit(async (valores) => {
    setErrorGeneral(null);
    try {
      await accederComoPaciente(valores.nombre.trim(), valores.apellido.trim(), valores.dni.trim());
      navegar('/mis-turnos', { replace: true });
    } catch (error) {
      setErrorGeneral(obtenerMensajeError(error));
    }
  });

  return (
    <Container size={420} my={80}>
      <Title ta="center" order={2}>
        Sacar un turno
      </Title>
      <Text c="dimmed" size="sm" ta="center" mt={5}>
        Ingresá tus datos. Si ya tenías un turno con nosotros, vas a entrar directo a tu cuenta.
      </Text>

      <Card withBorder shadow="sm" padding="lg" radius="md" mt="xl">
        <form onSubmit={manejarEnvio}>
          <Stack>
            <TextInput label="Nombre" required autoFocus {...form.getInputProps('nombre')} />
            <TextInput label="Apellido" required {...form.getInputProps('apellido')} />
            <TextInput
              label="DNI"
              placeholder="Sin puntos, ej: 30123456"
              required
              inputMode="numeric"
              {...form.getInputProps('dni')}
            />
            {errorGeneral && (
              <Text c="red" size="sm" role="alert">
                {errorGeneral}
              </Text>
            )}
            <Button type="submit" fullWidth loading={cargando}>
              Continuar
            </Button>
          </Stack>
        </form>
      </Card>

      <Text ta="center" size="sm" mt="md">
        ¿Sos administrador o profesional? <Anchor component={Link} to="/login">Iniciá sesión acá</Anchor>
      </Text>
    </Container>
  );
}
