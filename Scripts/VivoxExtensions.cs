namespace Insthync.UnityVivoxIntegration
{
    public static class VivoxExtensions
    {
        public static string GetSIP(this VivoxChannelType channelType)
        {
            switch (channelType)
            {
                case VivoxChannelType.Positional:
                    return "confctl-d-";
                case VivoxChannelType.Echo:
                    return "confctl-e-";
            }
            return "confctl-g-";
        }

        public static VivoxChannelType GetChannelType(this string channelUri)
        {
            if (channelUri.StartsWith("sip:confctl-d-") || channelUri.StartsWith("confctl-d-"))
                return VivoxChannelType.Positional;
            else if (channelUri.StartsWith("sip:confctl-e-") || channelUri.StartsWith("confctl-e-"))
                return VivoxChannelType.Echo;
            return VivoxChannelType.NonPositional;
        }
    }
}
