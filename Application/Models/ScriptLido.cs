namespace Application.Models;

public class ScriptLido
{
    public string NomeArquivo { get; set; } = string.Empty;
    public string CaminhoCompleto { get; set; } = string.Empty;

    // Remove a extensão ".cs" do nome para exibir um título mais limpo no menu do Spectre
    public string NomeAmigavel => NomeArquivo.Replace(".cs", "", StringComparison.OrdinalIgnoreCase);
}