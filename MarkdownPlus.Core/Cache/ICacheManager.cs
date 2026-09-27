namespace MarkdownPlus.Core.Cache;

public interface ICacheManager
{
    Task<CacheEntry?> GetAsync(string key, CancellationToken cancellationToken = default);

    Task SetAsync(
        string key,
        ReadOnlyMemory<byte> data,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> ListKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca <paramref name="key"/> como "em uso" agora, sem precisar ler os dados.
    /// Se <paramref name="extendExpirationBy"/> for informado, a expiração é renovada
    /// a partir de agora (ex.: manter um card vivo enquanto o jogo continuar aparecendo
    /// na lista do dia).
    /// </summary>
    Task TouchAsync(
        string key,
        TimeSpan? extendExpirationBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>Remove todas as entradas cuja validade (ExpiresAt) já passou. Retorna as chaves removidas.</summary>
    Task<IReadOnlyCollection<string>> PurgeExpiredAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove entradas não acessadas/tocadas há mais que <paramref name="unusedFor"/>,
    /// mesmo que não tenham uma expiração explícita definida.
    /// </summary>
    Task<IReadOnlyCollection<string>> PurgeUnusedAsync(TimeSpan unusedFor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Caminho físico do arquivo de dados de <paramref name="key"/>, quando a implementação
    /// for baseada em arquivos (ex.: <c>FileCacheManager</c>); <c>null</c> caso contrário.
    /// Necessário quando o conteúdo cacheado precisa ser referenciado por caminho
    /// (ex.: um &lt;img src&gt; / srcset no markdown gerado) e não só lido em memória.
    /// </summary>
    string? TryGetFilePath(string key);
}
