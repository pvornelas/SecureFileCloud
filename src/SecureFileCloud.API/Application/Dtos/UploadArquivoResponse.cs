namespace SecureFileCloud.API.Application.Dtos;

public sealed record UploadArquivoResponse(
    Guid Id,
    string NomeOriginal,
    string ContentType,
    long TamanhoEmBytes,
    DateTime DataUpload
);