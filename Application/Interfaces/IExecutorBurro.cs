namespace Application.Interfaces
{
    public interface IExecutorBurro
    {
        Task<string> ExecutarAsync(
            string scriptConteudo,
            Action<string>? onLineRead = null,
            CancellationToken ct = default    
        );
    }
}
