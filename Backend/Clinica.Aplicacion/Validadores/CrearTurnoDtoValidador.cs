using Clinica.Aplicacion.Comun;
using Clinica.Aplicacion.DTOs.Turnos;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class CrearTurnoDtoValidador : AbstractValidator<CrearTurnoDto>
{
    public CrearTurnoDtoValidador()
    {
        RuleFor(t => t.PacienteId)
            .GreaterThan(0).WithMessage("Debe seleccionar un paciente.");

        RuleFor(t => t.ProfesionalId)
            .GreaterThan(0).WithMessage("Debe seleccionar un profesional.");

        // No se permite agendar turnos nuevos en fechas pasadas.
        RuleFor(t => t.Fecha)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("La fecha del turno no puede ser anterior a hoy.");

        RuleFor(t => t.Horario)
            .InclusiveBetween(HorarioClinica.Apertura, HorarioClinica.Cierre)
            .WithMessage($"El horario debe estar entre las {HorarioClinica.Apertura:HH\\:mm} y las {HorarioClinica.Cierre:HH\\:mm}.");
    }
}
