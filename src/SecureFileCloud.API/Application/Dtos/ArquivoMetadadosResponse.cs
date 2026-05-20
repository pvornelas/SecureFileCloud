namespace SecureFileCloud.API.Application.Dtos;

public sealed record ArquivoMetadadosResponse(
    Guid Id,
    string NomeOriginal,
    string ContentType,
    long TamanhoEmBytes,
    DateTime DataUpload
);