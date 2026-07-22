// =============================================================================
// CAMADA: Application (Regras de negócio — depende apenas do Domain)
// ARQUIVO: Argon2Helper.cs
// =============================================================================
// CONCEITO — Por que hash de senha?
// Nunca guardamos a senha do usuário em texto puro no banco. Se o banco
// vazar, as senhas não podem ser recuperadas. Em vez disso, aplicamos uma
// função de hash unidirecional: é fácil ir da senha para o hash, mas
// computacionalmente impossível voltar do hash para a senha.
//
// Por que Argon2id e não MD5/SHA256?
// MD5 e SHA256 são muito rápidos — um atacante pode testar bilhões de
// combinações por segundo em uma GPU. O Argon2id é propositalmente LENTO
// e consome muita memória RAM (64MB aqui), tornando ataques de força bruta
// impraticáveis mesmo com hardware moderno. É o algoritmo vencedor do
// Password Hashing Competition (2015) e o estado da arte em segurança.
//
// CONCEITO — Salt:
// Um "salt" é um valor aleatório gerado para cada senha antes de calcular
// o hash. Isso garante que dois usuários com a mesma senha ("admin") tenham
// hashes completamente diferentes no banco, impedindo "rainbow table attacks"
// (tabelas pré-calculadas de hashes comuns).
// =============================================================================

using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace Application.Security
{
    public static class Argon2Helper
    {
        // Parâmetros de "custo" do Argon2id — quanto maior, mais seguro e mais lento.
        // Estes valores são escolhas deliberadas de segurança, não arbitrárias.
        private const int MemorySize = 65536;       // 64 MB de RAM por operação de hash
        private const int Iterations = 4;           // 4 passagens pelo algoritmo
        private const int DegreeOfParallelism = 2;  // Usa 2 threads em paralelo
        private const int SaltSize = 16;            // 128 bits de salt (suficiente para ser único)
        private const int HashSize = 32;            // 256 bits de hash resultante

        // Gera um hash seguro para uma senha nova.
        // Chamado ao cadastrar um usuário ou alterar a senha.
        //
        // Formato do hash salvo: "$argon2id$v=19$m=65536,t=4,p=2$<salt_base64>$<hash_base64>"
        // Os parâmetros ficam embutidos no hash, então a verificação funciona
        // mesmo que você mude os parâmetros no futuro (hashes antigos continuam válidos).
        public static string GerarHash(string senha)
        {
            try
            {
                // Gera um salt aleatório criptograficamente seguro.
                // RandomNumberGenerator é o gerador seguro do .NET — use sempre este
                // em vez de "new Random()" para fins de segurança.
                var salt = RandomNumberGenerator.GetBytes(SaltSize);

                // Configura o objeto Argon2id com os parâmetros definidos.
                // "using var" garante que a memória alocada pelo algoritmo seja
                // liberada assim que sair do escopo (padrão IDisposable).
                using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(senha))
                {
                    Salt = salt,
                    DegreeOfParallelism = DegreeOfParallelism,
                    MemorySize = MemorySize,
                    Iterations = Iterations,
                };

                var hash = argon2.GetBytes(HashSize);

                // Retorna uma string formatada contendo o algoritmo, versão,
                // parâmetros, salt e hash — tudo necessário para verificar depois.
                return $"$argon2id$v=19$m={MemorySize},t={Iterations},p={DegreeOfParallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
            }
            catch (Exception ex)
            {
                return $"Erro: {ex.Message}";
            }
        }

        // Verifica se uma senha digitada corresponde ao hash salvo no banco.
        // Extrai os parâmetros do hash salvo, recalcula o hash com a senha
        // fornecida, e compara os resultados.
        public static bool VerificarSenha(string senhaDigitada, string hashSalvo)
        {
            try
            {
                // Divide o hash salvo pelos separadores "$" para extrair cada parte.
                // Formato esperado: ["", "argon2id", "v=19", "m=...,t=...,p=...", "<salt>", "<hash>"]
                var partes = hashSalvo.Split('$');
                if (partes.Length < 6) return false; // Hash malformado — rejeita

                // Extrai os parâmetros de custo do segmento "m=65536,t=4,p=2"
                var config = partes[3];
                var memSize = int.Parse(config.Split(',')[0].Split('=')[1]);
                var iterations = int.Parse(config.Split(',')[1].Split('=')[1]);
                var parallelism = int.Parse(config.Split(',')[2].Split('=')[1]);

                // Reconstrói o salt e o hash original a partir de Base64
                var salt = Convert.FromBase64String(partes[4]);
                var hashOriginal = Convert.FromBase64String(partes[5]);

                // Recalcula o hash da senha digitada usando os MESMOS parâmetros
                // que foram usados ao gerar o hash original.
                using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(senhaDigitada))
                {
                    Salt = salt,
                    DegreeOfParallelism = parallelism,
                    MemorySize = memSize,
                    Iterations = iterations
                };

                var hashNovaSenha = argon2.GetBytes(hashOriginal.Length);

                // CONCEITO — Comparação em tempo constante:
                // Usar "==" ou "SequenceEqual" para comparar arrays de bytes é
                // vulnerável a "timing attacks": o atacante pode medir o tempo
                // de resposta e descobrir quantos bytes batem antes de falhar.
                // FixedTimeEquals sempre leva o mesmo tempo, independente do resultado.
                return CryptographicOperations.FixedTimeEquals(hashOriginal, hashNovaSenha);
            }
            catch (Exception)
            {
                // Se qualquer parse falhar (hash corrompido, formato errado),
                // simplesmente retorna false em vez de lançar uma exceção.
                return false;
            }
        }
    }
}
