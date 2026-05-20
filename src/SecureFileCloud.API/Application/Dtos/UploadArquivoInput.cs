namespace SecureFileCloud.API.Application.Dtos;

public sealed record UploadArquivoInput(
    string NomeOriginal,
    string ContentType,
    long TamanhoEmBytes,
    Func<Stream> AbrirStream
);