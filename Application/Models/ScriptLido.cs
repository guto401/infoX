// =============================================================================
// CAMADA: Application (Regras de negócio — depende apenas do Domain)
// ARQUIVO: ScriptLido.cs
// =============================================================================
// CONCEITO — DTO (Data Transfer Object):
// Um DTO é um objeto simples cuja única função é carregar dados de um lugar
// para outro. Ele não tem lógica de negócio, não faz operações — só agrupa
// informações relacionadas.
//
// ScriptLido é usado para transportar as informações de um arquivo .cs
// encontrado no disco até a interface do menu (Spectre.Console).
// Sem ele, teríamos que passar múltiplos parâmetros soltos pela aplicação.
// =============================================================================

namespace Application.Models;

public class ScriptLido
{
    // Nome do arquivo com extensão. Ex: "LimpezaTemp.cs"
    // Usado internamente para identificar e carregar o arquivo do disco.
    public string NomeArquivo { get; set; } = string.Empty;

    // Caminho absoluto no sistema de arquivos. Ex: "C:\InfoX\Scripts\LimpezaTemp.cs"
    // Mantemos o caminho completo para não precisar reconstruí-lo depois.
    public string CaminhoCompleto { get; set; } = string.Empty;

    // Propriedade calculada (expression body com "=>") — não armazena valor,
    // calcula na hora que é lida. Remove a extensão ".cs" para exibir um
    // nome mais limpo no menu. Ex: "LimpezaTemp.cs" → "LimpezaTemp"
    //
    // StringComparison.OrdinalIgnoreCase garante que ".CS" ou ".Cs"
    // também sejam removidos corretamente.
    public string NomeAmigavel => NomeArquivo.Replace(".cs", "", StringComparison.OrdinalIgnoreCase);
}