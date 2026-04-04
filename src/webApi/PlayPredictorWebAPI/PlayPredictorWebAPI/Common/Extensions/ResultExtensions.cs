using AutoMapper;
using g_map_compare_backend.Common.Results;
using Microsoft.AspNetCore.Mvc;
using PlayPredictorWebAPI.Common.Results;

public static class ResultExtensions
{
    // Einzelnes Objekt Domain → DTO
    public static ActionResult<TDto> ToActionResult<TDomain, TDto>(
        this Result<TDomain> result,
        ControllerBase controller,
        IMapper mapper,
        string? createdActionName = null,
        object? createdRouteValues = null)
    {
        if (result.Success)
        {
            var dto = mapper.Map<TDto>(result.Data);

            if (createdActionName != null && createdRouteValues != null)
                return controller.CreatedAtAction(createdActionName, createdRouteValues, dto);

            return controller.Ok(dto);
        }

        return MapError(result, controller);
    }

    public static ActionResult<TDto> ToActionResult<TDto>(
        this Result<TDto> result,
        ControllerBase controller,
        string? createdActionName = null,
        object? createdRouteValues = null)
    {
        if (result.Success)
        {
            if (createdActionName != null && createdRouteValues != null)
                return controller.CreatedAtAction(createdActionName, createdRouteValues, result.Data);

            return controller.Ok(result.Data);
        }

        return MapError(result, controller);
    }

    // Liste von Objekten Domain → DTO
    public static ActionResult<IEnumerable<TDto>> ToActionResult<TDomain, TDto>(
        this Result<IEnumerable<TDomain>> result,
        ControllerBase controller,
        IMapper mapper)
    {
        if (result.Success)
        {
            var dtoList = mapper.Map<IEnumerable<TDto>>(result.Data);
            return controller.Ok(dtoList);
        }

        return MapError(result, controller);
    }

    public static ActionResult<ICollection<TDto>> ToActionResult<TDomain, TDto>(
        this Result<ICollection<TDomain>> result,
        ControllerBase controller,
        IMapper mapper)
    {
        if (result.Success)
        {
            var dtoList = mapper.Map<ICollection<TDto>>(result.Data);
            return controller.Ok(dtoList);
        }

        return MapError(result, controller);
    }

    // Nur Status zurückgeben (z.B. Delete)
    public static ActionResult ToActionResult(
        this Result result,
        ControllerBase controller)
    {
        if (result.Success)
            return controller.NoContent();

        return MapError(result, controller);
    }

    // interne Hilfsmethode für Fehler
    private static ActionResult MapError<TDomain>(Result<TDomain> result, ControllerBase controller)
    {
        return result.ErrorCode switch
        {
            ErrorCode.ValidationError => controller.BadRequest(GetProblemDetails(result.ErrorMessage)),
            ErrorCode.NotFound => controller.NotFound(GetProblemDetails(result.ErrorMessage)),
            ErrorCode.Conflict => controller.Conflict(GetProblemDetails(result.ErrorMessage)),
            ErrorCode.Unauthorized => controller.Unauthorized(GetProblemDetails(result.ErrorMessage)),
            ErrorCode.Forbidden => controller.Forbid(),
            _ => controller.StatusCode(500, GetProblemDetails(result.ErrorMessage) ?? new ProblemDetails { Title = ErrorMessage.GetDescription(ErrorType.UNKNOWN)})
        };
    }

    private static ActionResult MapError(Result result, ControllerBase controller)
    {
        return result.ErrorCode switch
        {
            ErrorCode.ValidationError => controller.BadRequest(GetProblemDetails(result.ErrorMessage)),
            ErrorCode.NotFound => controller.NotFound(GetProblemDetails(result.ErrorMessage)),
            ErrorCode.Conflict => controller.Conflict(GetProblemDetails(result.ErrorMessage)),
            ErrorCode.Unauthorized => controller.Unauthorized(GetProblemDetails(result.ErrorMessage)),
            ErrorCode.Forbidden => controller.Forbid(),
            _ => controller.StatusCode(500, GetProblemDetails(result.ErrorMessage) ?? new ProblemDetails { Title = ErrorMessage.GetDescription(ErrorType.UNKNOWN) })
        };
    }

    private static ProblemDetails GetProblemDetails(ErrorMessage errorMessage)
    {
        return new ProblemDetails
        {
            Title = ErrorMessage.GetDescription(errorMessage.Type),
            Detail = errorMessage.Details
        };
    }
}
