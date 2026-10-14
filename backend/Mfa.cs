using System.Security.Cryptography; // Le TOTP repose sur la signature HMAC du compteur temporel.
using System.Text; // Construit le secret Base32 accepté par les applications d'authentification.

namespace Iai; // Module de second facteur administratif.
public static class Mfa // Implémente le TOTP standard RFC 6238 à six chiffres et trente secondes.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567"; // Le secret est présenté dans l'alphabet Base32 sans caractères ambigus.
    public static string NewSecret() // Génère un secret de vingt octets pour chaque administrateur.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var bytes = RandomNumberGenerator.GetBytes(20); // La source cryptographique est indépendante du mot de passe.
        var result = new StringBuilder(); // Construit l'encodage partageable avec l'application TOTP.
        int buffer = 0, bits = 0; // Conserve les bits restants entre deux octets.
        foreach (var b in bytes) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            buffer = (buffer << 8) | b; // Renseigne le champ buffer avec la valeur calculée ou validée à droite.
            bits += 8; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            while (bits >= 5) // Répète le traitement tant que la condition reste vraie, notamment pendant la lecture du CSV.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                bits -= 5; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                result.Append(Alphabet[(buffer >> bits) & 31]); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            } // Ferme ce bloc après l’exécution de ses instructions.
        } // Encode les groupes successifs de cinq bits.

        return result.ToString(); // Vingt octets produisent exactement trente-deux caractères.
    } // Ferme ce bloc après l’exécution de ses instructions.

    public static string Code(string secret, long counter) // Calcule le code d'une fenêtre temporelle donnée.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        int buffer = 0, bits = 0; // Calcule ou prépare la variable buffer, utilisée par les instructions suivantes.
        var key = new List<byte>(); // Prépare le décodage Base32 du secret.
        foreach (var c in secret) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            var value = Alphabet.IndexOf(c); // Calcule ou prépare la variable value, utilisée par les instructions suivantes.
            if (value < 0) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Secret MFA invalide."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            buffer = (buffer << 5) | value; // Renseigne le champ buffer avec la valeur calculée ou validée à droite.
            bits += 5; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            if (bits >= 8) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                bits -= 8; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                key.Add((byte)(buffer >> bits)); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            } // Ferme ce bloc après l’exécution de ses instructions.
        } // Reconstitue les octets de la clé HMAC.

        var bytes = BitConverter.GetBytes(counter); // Calcule ou prépare la variable bytes, utilisée par les instructions suivantes.
        if (BitConverter.IsLittleEndian) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            Array.Reverse(bytes); // Le compteur est encodé sur huit octets en ordre réseau.
        var hash = HMACSHA1.HashData(key.ToArray(), bytes); // Le standard TOTP utilise ici HMAC-SHA1.
        var offset = hash[^1] & 15; // La troncature dynamique sélectionne quatre octets de la signature.
        var value32 = ((hash[offset] & 127) << 24) | (hash[offset + 1] << 16) | (hash[offset + 2] << 8) | hash[offset + 3]; // Supprime le bit de signe pour un entier positif.
        return (value32 % 1_000_000).ToString("D6"); // Formate systématiquement le code sur six chiffres.
    } // Ferme ce bloc après l’exécution de ses instructions.

    public static long Validate(string secret, string code, long lastStep) // Une fenêtre déjà consommée ne peut pas être rejouée.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var now = PortalClock.Now.ToUnixTimeSeconds() / 30; // Les applications mobiles utilisent des fenêtres de trente secondes.
        foreach (var step in new[] // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            now, // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            now - 1, // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            now + 1 // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
        } // Ferme ce bloc après l’exécution de ses instructions.

        ) // Termine cette construction et revient à l’instruction qui l’utilise.
            if (step > lastStep && Code(secret, step) == code) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                return step; // Accepte une faible dérive d'horloge sans réutiliser de code.
        throw new RuleException("Code MFA incorrect ou déjà utilisé.", 401); // Aucun cookie n'est créé avant validation des deux facteurs.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.
