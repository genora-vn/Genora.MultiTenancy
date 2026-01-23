namespace Genora.MultiTenancy.Enums;

public enum EmailStatus : byte
{
    Pending = 0,
    Sending = 1,
    Sent = 2,
    Failed = 3,
    Abandoned = 4
}