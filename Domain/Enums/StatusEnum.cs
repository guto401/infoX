// =============================================================================
// CAMADA: Domain (O núcleo da cebola — não depende de NADA externo)
// ARQUIVO: StatusEnum.cs
// =============================================================================
// Um Enum é um tipo que representa um conjunto fixo de valores nomeados.
// Em vez de usar strings soltas como "Concluido" ou "Erro" espalhadas pelo código
// (o que causa erros de digitação e dificulta buscas), usamos um enum.
// Isso garante que o compilador valide os valores — se você digitar errado, não compila.
//
// Este enum vive no Domain porque ele representa um conceito de NEGÓCIO:
// o estado de uma execução. Ele não tem nada de infraestrutura, banco ou UI.
// =============================================================================

namespace Domain.Enums
{
    public enum StatusEnum
    {
        Concluido,  // O script foi executado e terminou sem erros detectados
        Rodando,    // Reservado para uso futuro (ex: execuções assíncronas longas)
        Cancelado,  // O usuário cancelou a operação dentro do sub-menu do script
        Erro        // Ocorreu uma falha durante a execução (seja no Roslyn ou no PowerShell)
    }
}
