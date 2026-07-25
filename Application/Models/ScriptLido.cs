namespace Application.Models;

public class ScriptLido
{
    public string NomeArquivo { get; set; } = string.Empty;

    public string CaminhoCompleto { get; set; } = string.Empty;

    public string NomeAmigavel => NomeArquivo.Replace(".cs", "", StringComparison.OrdinalIgnoreCase);
}