using System.Collections.Generic;

namespace Genora.MultiTenancy.AuditLogs;

public class EntityChangeDto
{
    public string EntityTypeFullName { get; set; }
    public string ChangeType { get; set; }
    public string EntityId { get; set; }
    public List<EntityPropertyChangeDto> PropertyChanges { get; set; } = new();
}