public class IPTools
{
    public static bool IsValidIP(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return false;

        string[] parts = ip.Split('.');
        if (parts.Length != 4) return false;

        foreach (string part in parts)
        {
            if (!int.TryParse(part, out int value) || value < 0 || value > 255)
                return false;
        }

        return true;
    }

}
