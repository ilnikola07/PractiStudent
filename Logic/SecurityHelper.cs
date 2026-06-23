using System.Security.Cryptography;
using System.Text;

namespace Logic
{
    public static class SecurityHelper // класс для хеширования паролей
    {
        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create()) //создаёт криптографический объект для вычисления хеша 
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) // преобразует каждый байт в двузначное шестнадцатеричное число и склеивает их в одну строку
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
