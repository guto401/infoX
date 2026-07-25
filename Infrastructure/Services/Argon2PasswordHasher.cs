using Application.Interfaces;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services
{
    public class Argon2PasswordHasher : IPasswordHasher
    {
        private const int MemorySize = 65536;
        private const int Iterations = 4;
        private const int DegreeOfParallelism = 2;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public string GerarHash(string senha)
        {
            try
            {
                var salt = RandomNumberGenerator.GetBytes(SaltSize);

                using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(senha))
                {
                    Salt = salt,
                    DegreeOfParallelism = DegreeOfParallelism,
                    MemorySize = MemorySize,
                    Iterations = Iterations,
                };

                var hash = argon2.GetBytes(HashSize);

                return $"$argon2id$v=19$m={MemorySize},t={Iterations},p={DegreeOfParallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
            }
            catch (Exception ex)
            {
                return $"Erro: {ex.Message}";
            }
        }

        public bool VerificarSenha(string senhaDigitada, string hashSalvo)
        {
            try
            {
                var partes = hashSalvo.Split('$');
                if (partes.Length < 6) return false;

                var config = partes[3];
                var memSize = int.Parse(config.Split(',')[0].Split('=')[1]);
                var iterations = int.Parse(config.Split(',')[1].Split('=')[1]);
                var parallelism = int.Parse(config.Split(',')[2].Split('=')[1]);

                var salt = Convert.FromBase64String(partes[4]);
                var hashOriginal = Convert.FromBase64String(partes[5]);

                using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(senhaDigitada))
                {
                    Salt = salt,
                    DegreeOfParallelism = parallelism,
                    MemorySize = memSize,
                    Iterations = iterations
                };

                var hashNovaSenha = argon2.GetBytes(hashOriginal.Length);

                return CryptographicOperations.FixedTimeEquals(hashOriginal, hashNovaSenha);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
