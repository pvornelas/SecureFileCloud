namespace SecureFileCloud.API.Application.Dtos;

public sealed record DownloadArquivoResponse(
    string NomeOriginal,
    string ContentType,
    byte[] Conteudo
);
