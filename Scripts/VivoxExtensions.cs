namespace MultiplayerARPG
{
    public static class VivoxExtensions
    {
        public static string GetSIP(this VivoxChannelType channelType)
        {
            switch (channelType)
            {
                case VivoxChannelType.NonPositional:
                    return "confctl-g-";
                case VivoxChannelType.Positional:
                    return "confctl-d-";
                case VivoxChannelType.Echo:
                    return "confctl-e-";
            }
            return "confctl-g-";
        }
    }
}
