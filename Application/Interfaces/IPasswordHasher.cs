namespace Application.Interfaces
{
    public interface IPasswordHasher
    {
        string GerarHash(string senha);
        bool VerificarSenha(string senhaDigitada, string hashAlvo);
    }
}
