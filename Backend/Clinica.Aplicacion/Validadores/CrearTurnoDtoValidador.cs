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
            .WithMessage($"El horario debe estar entre las {HorarioClinica.Apertura:HH\\:mm} y las {HorarioClinica.Cierre:HH\\:mm}.")
            // Si la fecha es hoy, tampoco se permite un horario que ya pasó
            // (la grilla de /horarios-disponibles ya los excluye, pero esto
            // cubre a quien le pegue directo a la API sin pasar por ahí).
            .Must((dto, horario) => dto.Fecha != DateOnly.FromDateTime(DateTime.Now) || horario > TimeOnly.FromDateTime(DateTime.Now))
            .WithMessage("No se puede agendar un turno en un horario que ya pasó.");
    }
}
