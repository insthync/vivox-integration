using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Insthync.UnityVivoxIntegration
{
    /// <summary>
    /// This did follow: https://docs.unity.com/ugs/en-us/manual/vivox-unity/manual/access-token-guide/generate-token-secure-server/example-c-sharp
    /// User SIP generate as `sip:.issuer.userid.@domain`
    /// Channel SIP generate as `sip:confctl-[g/d/e]-issuer.channelid@domain`
    /// </summary>
    public class VivoxTokenGenerator
    {
        public static string GetUserSIP(string domain, string issuer, string userId)
        {
            return $"sip:.{issuer}.{userId}.@{domain}";
        }

        public static string GetChannelSIP(string domain, string issuer, VivoxChannelType channelType, string channelId)
        {
            return $"sip:{channelType.GetSIP()}{issuer}.{channelId}@{domain}";
        }

        public static string GenerateLoginToken(string domain, string issuer, string key, string userId, int expirationInSeconds = 90)
        {
            string vxa = "login";
            uint vxi = 0;
            string f = GetUserSIP(domain, issuer, userId);
            string t = string.Empty;
            string sub = string.Empty;
            return GenerateVivoxAccessToken(issuer, key, vxa, vxi, f, t, sub, expirationInSeconds);
        }

        public static string GenerateJoinToken(string domain, string issuer, string key, string userId, VivoxChannelType channelType, string channelId, int expirationInSeconds = 90)
        {
            string vxa = "join";
            uint vxi = 0;
            string f = GetUserSIP(domain, issuer, userId);
            string t = GetChannelSIP(domain, issuer, channelType, channelId);
            string sub = string.Empty;
            return GenerateVivoxAccessToken(issuer, key, vxa, vxi, f, t, sub, expirationInSeconds);
        }

        public static string GenerateJoinMutedToken(string domain, string issuer, string key, string userId, VivoxChannelType channelType, string channelId, int expirationInSeconds = 90)
        {
            string vxa = "join_muted";
            uint vxi = 0;
            string f = GetUserSIP(domain, issuer, userId);
            string t = GetChannelSIP(domain, issuer, channelType, channelId);
            string sub = string.Empty;
            return GenerateVivoxAccessToken(issuer, key, vxa, vxi, f, t, sub, expirationInSeconds);
        }

        public static string GenerateKickToken(string domain, string issuer, string key, string userId, string kickedUserId, VivoxChannelType channelType, string channelId, int expirationInSeconds = 90)
        {
            string vxa = "kick";
            uint vxi = 0;
            string f = GetUserSIP(domain, issuer, userId);
            string t = GetChannelSIP(domain, issuer, channelType, channelId);
            string sub = GetUserSIP(domain, issuer, kickedUserId);
            return GenerateVivoxAccessToken(issuer, key, vxa, vxi, f, t, sub, expirationInSeconds);
        }

        public static string GenerateMuteToken(string domain, string issuer, string key, string userId, string mutedUserId, VivoxChannelType channelType, string channelId, int expirationInSeconds = 90)
        {
            string vxa = "mute";
            uint vxi = 0;
            string f = GetUserSIP(domain, issuer, userId);
            string t = GetChannelSIP(domain, issuer, channelType, channelId);
            string sub = GetUserSIP(domain, issuer, mutedUserId);
            return GenerateVivoxAccessToken(issuer, key, vxa, vxi, f, t, sub, expirationInSeconds);
        }

        public static string GenerateVivoxAccessToken(string issuer, string key, string vxa, uint vxi, string f, string t, string sub, int expirationInSeconds = 90)
        {
            List<string> segments = new List<string>();
            // Header is static - base64url encoded {} - Can also be defined as a constant "e30"
            // Header is empty string, as mentioned here: https://docs.unity.com/ugs/en-us/manual/vivox-unity/manual/access-token-guide/access-token-format/access-token-header
            var header = Base64URLEncode("{}");
            segments.Add(header);

            // Encode payload
            // Payload: https://docs.unity.com/ugs/en-us/manual/vivox-unity/manual/access-token-guide/access-token-format/access-token-payload
            var claims = new Claims
            {
                iss = issuer,
                exp = (int)ToUnixEpochTime(DateTime.UtcNow.AddSeconds(expirationInSeconds)),
                vxa = vxa,
                vxi = vxi,
                f = f,
                t = t,
                sub = sub,
            };
            var claimsString = JsonConvert.SerializeObject(claims);
            var encodedClaims = Base64URLEncode(claimsString);

            // Join segments to prepare for signing
            segments.Add(encodedClaims);
            string toSign = string.Join(".", segments);

            // Sign token with key and SHA256
            string sig = SHA256Hash(key, toSign);
            segments.Add(sig);

            // Join all 3 parts of token with . and return
            string token = string.Join(".", segments);

            return token;
        }


        private static string Base64URLEncode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            // Remove padding at the end
            var encodedString = System.Convert.ToBase64String(plainTextBytes).TrimEnd('=');
            // Substitute URL-safe characters
            string urlEncoded = encodedString.Replace("+", "-").Replace("/", "_");

            return urlEncoded;
        }

        private static string SHA256Hash(string secret, string message)
        {
            var encoding = new ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            // The instance of HMACSHA256 is constructed and disposed in this method because it is not thread safe
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                var hashString = Convert.ToBase64String(hashmessage).TrimEnd('=');
                string urlEncoded = hashString.Replace("+", "-").Replace("/", "_");

                return urlEncoded;
            }
        }

        private static long ToUnixEpochTime(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// https://docs.unity.com/ugs/en-us/manual/vivox-unity/manual/access-token-guide/access-token-format/access-token-payload
        /// </summary>
        public class Claims
        {
            public string iss { get; set; }
            /// <summary>
            /// The expiration time as epoch seconds.
            /// </summary>
            public int exp { get; set; }
            /// <summary>
            /// Action - https://docs.unity.com/ugs/en-us/manual/vivox-unity/manual/access-token-guide/access-token-format/supported-values-vxa-claim
            /// </summary>
            public string vxa { get; set; }
            /// <summary>
            /// Token uniqueness
            /// </summary>
            public uint vxi { get; set; }
            /// <summary>
            /// From (Who want to kick)
            /// </summary>
            public string f { get; set; }
            /// <summary>
            /// To (In which channel)
            /// </summary>
            public string t { get; set; }
            /// <summary>
            /// Subject (Who is being kicked)
            /// </summary>
            public string sub { get; set; }
        }
    }
}
