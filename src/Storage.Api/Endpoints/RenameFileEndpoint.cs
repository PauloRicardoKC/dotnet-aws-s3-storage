using FluentValidation;
using Storage.Application.DTOs.Requests;
using Storage.Application.DTOs.Responses;
using Storage.Application.Services.Interfaces;

namespace Storage.Api.Endpoints;

public static class RenameFileEndpoint
{
    public static RouteGroupBuilder MapRenameFileEndpoint(this RouteGroupBuilder group)
    {
        group.MapPut("/rename", async (RenameFileRequest request, IStorageService service,
                IValidator<RenameFileRequest> validator, CancellationToken cancellationToken) =>
            {
                var validation = await validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(errors => errors.Key,
                            errors => errors.Select(error => error.ErrorMessage).ToArray()));

                return Results.Ok(await service.RenameAsync(request, cancellationToken));
            })
            .WithSummary("Renames a file by copying it and deleting the original")
            .WithDescription("Request example: { \"oldKey\": \"products/old.pdf\", \"newKey\": \"products/new.pdf\" }. Response example: { \"oldKey\": \"products/old.pdf\", \"newKey\": \"products/new.pdf\" }.")
            .Produces<RenameFileResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();
        return group;
    }
}
