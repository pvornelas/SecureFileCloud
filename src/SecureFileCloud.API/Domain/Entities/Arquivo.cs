namespace SecureFileCloud.API.Domain.Entities;

public class Arquivo
{
    public Guid Id { get; init; }
    public string NomeOriginal { get; init; } = default!;
    public string NomeArmazenado { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public long TamanhoEmBytes { get; init; }
    public DateTime DataUpload { get; init; }

    private Arquivo() { }

    public static Arquivo Create(string nomeOriginal, string nomeArmazenado, string contentType, long tamanhoEmBytes)
    {
        return new Arquivo
        {
            Id = Guid.NewGuid(),
            NomeOriginal = nomeOriginal,
            NomeArmazenado = nomeArmazenado,
            ContentType = contentType,
            TamanhoEmBytes = tamanhoEmBytes,
            DataUpload = DateTime.UtcNow
        };
    }
}
