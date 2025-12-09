using System;

namespace Genora.MultiTenancy.AuditLogs;

public sealed class AuditLogCleanupOptions
{
    public bool Enabled { get; set; } = true;
    public TimeSpan Period { get; set; } = TimeSpan.FromHours(6);
    public TimeSpan Retention { get; set; } = TimeSpan.FromDays(30);
    public int BatchSize { get; set; } = 0;
}