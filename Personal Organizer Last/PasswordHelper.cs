using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public static class PasswordHelper
{
    private static readonly char[] Letters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
        .ToCharArray();
    private static readonly char[] Digits =
        "0123456789".ToCharArray();
    private static readonly char[] AllChars =
        Letters.Concat(Digits).ToArray();

    public static string Generate(int length)
    {
        if (length < 1)
            throw new ArgumentException("Length must be at least 1", nameof(length));

        var rng = RandomNumberGenerator.Create();
        var passwordChars = new List<char>(length);

        // 1. En az bir rakam ekle
        passwordChars.Add(GetRandomChar(Digits, rng));

        // 2. Kalanı tüm karakter setinden rastgele doldur
        for (int i = 1; i < length; i++)
        {
            passwordChars.Add(GetRandomChar(AllChars, rng));
        }

        // 3. Listeyi karıştır (Fisher–Yates shuffle)
        Shuffle(passwordChars, rng);

        return new string(passwordChars.ToArray());
    }

    private static char GetRandomChar(char[] set, RandomNumberGenerator rng)
    {
        byte[] b = new byte[1];
        rng.GetBytes(b);
        // Mod alarak indis seç
        return set[b[0] % set.Length];
    }

    private static void Shuffle<T>(IList<T> list, RandomNumberGenerator rng)
    {
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            rng.GetBytes(box);
            int k = box[0] % n;  // 0 ≤ k < n
            n--;
           
            T tmp = list[k];
            list[k] = list[n];
            list[n] = tmp;
        }
    }
}
