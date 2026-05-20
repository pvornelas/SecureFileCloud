namespace SecureFileCloud.API.Application.Interfaces;
public sealed record ArquivoArmazenado(
    string NomeOriginal,
    string NomeArmazenado,
    string ContentType,
    long TamanhoEmBytes
);

public sealed record ArquivoParaArmazenar(
    string NomeOriginal,
    string ContentType,
    long TamanhoEmBytes,
    Stream Conteudo
);

public interface IArquivoStorage
{
    Task<ArquivoArmazenado> SalvarAsync(ArquivoParaArmazenar arquivo, CancellationToken cancellationToken = default);

    Task<byte[]?> LerBytesAsync(string nomeArmazenado, CancellationToken cancellationToken = default);

    bool Excluir(string nomeArmazenado);

    bool Existe(string nomeArmazenado);
}
