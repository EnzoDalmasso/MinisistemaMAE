using Clinica.Aplicacion.DTOs.Profesionales;
using FluentValidation;

namespace Clinica.Aplicacion.Validadores;

public class ActualizarHorariosDtoValidador : AbstractValidator<ActualizarHorariosDto>
{
    public ActualizarHorariosDtoValidador()
    {
        RuleForEach(dto => dto.Bloques).SetValidator(new BloqueHorarioDtoValidador());

        RuleFor(dto => dto.Bloques)
            .Must(bloques => !HaySolapamiento(bloques))
            .WithMessage("Hay bloques horarios superpuestos para el mismo día.");
    }

    // Dos bloques del mismo día se superponen si, ordenados por hora de
    // inicio, alguno empieza antes de que termine el anterior.
    private static bool HaySolapamiento(List<BloqueHorarioDto> bloques) =>
        bloques
            .GroupBy(b => b.DiaSemana)
            .Any(grupoDelDia =>
            {
                var ordenados = grupoDelDia.OrderBy(b => b.HoraInicio).ToList();
                for (var i = 1; i < ordenados.Count; i++)
                {
                    if (ordenados[i].HoraInicio < ordenados[i - 1].HoraFin)
                    {
                        return true;
                    }
                }
                return false;
            });
}
