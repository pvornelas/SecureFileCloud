namespace SecureFileCloud.API.Application.Dtos;

public sealed record DownloadMultiplosResponse(
    string NomeArquivo,
    string ContentType,
    byte[] Conteudo
);
