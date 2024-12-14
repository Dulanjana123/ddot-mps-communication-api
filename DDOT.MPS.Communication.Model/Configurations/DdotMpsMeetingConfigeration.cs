namespace DDOT.MPS.Communication.Model.Configurations
{
    public class DdotMpsMeetingConfigeration
    {
        public const string SectionName = "DdotMpsMeetingSetings";

        public required string GraphApiAppTenantId { get; set; }
        public required string GraphApiAppClientId { get; set; }
        public required string GraphApiAppClientSecret { get; set; }
        public required string OrganizerEmail { get; set; }
    }
}
