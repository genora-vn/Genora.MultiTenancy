namespace Genora.MultiTenancy.AuditLogs;

public class EntityPropertyChangeDto
{
    public string PropertyName { get; set; }
    public string NewValue { get; set; }
    public string OriginalValue { get; set; }
}
