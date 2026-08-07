using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Storage.Application.DTOs.Requests;
using Storage.Application.Services.Interfaces;

namespace Storage.Api.Endpoints;

public static class UploadEndpoint
{
    public static RouteGroupBuilder MapUploadEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/upload",
            async (IFormFile? file, [FromForm] string? folder, IStorageService service, IValidator<UploadFileRequest> validator,
                CancellationToken cancellationToken) =>
            {
                if (file is null)
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    { ["file"] = ["A file is required."] });

                await using var stream = file.OpenReadStream();
                
                var request = new UploadFileRequest
                {
                    FileName = file.FileName,
                    Folder = folder,
                    ContentType = file.ContentType ?? "application/octet-stream",
                    Content = stream,
                    Length = file.Length
                };
                
                var validation = await validator.ValidateAsync(request, cancellationToken);

                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(group => group.Key,
                            group => group.Select(error => error.ErrorMessage).ToArray()));

                var response = await service.UploadAsync(request, cancellationToken);

                return Results.Created($"/files/{response.Key}", response);
            }).WithSummary("Uploads a file to the storage provider").DisableAntiforgery();
        return group;
    }
}
