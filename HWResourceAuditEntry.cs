using System;

namespace HaloWarsTools
{
    public enum HWResourceAuditEntryType
    {
        Created,
        Accessed
    }

    public struct HWResourceAuditEntry
    {
        public HWResource Resource;
        public DateTime When;
        public HWResourceAuditEntryType Type;

        public HWResourceAuditEntry(HWResource resource, HWResourceAuditEntryType type) {
            When = DateTime.Now;
            Resource = resource;
            Type = type;
        }

        public override string ToString() {
            string text = Type switch {
                HWResourceAuditEntryType.Created => $"Created resource \"{Resource.UserFriendlyName}\"",
                HWResourceAuditEntryType.Accessed => $"Accessed resource \"{Resource.UserFriendlyName}\"",
                _ => null
            };

            return $"{When.ToShortTimeString()}: {text}";
        }
    }
}
