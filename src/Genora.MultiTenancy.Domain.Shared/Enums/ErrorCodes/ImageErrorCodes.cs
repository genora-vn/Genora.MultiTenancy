namespace Genora.MultiTenancy.Enums.ErrorCodes;
public static class ImageErrorCodes
{
    public const string Prefix = "Image:";

    public const string FileRequired = Prefix + "FileRequired";
    public const string InvalidExtension = Prefix + "InvalidExtension";
    public const string DeleteFailed = Prefix + "DeleteFailed";
    public const string UploadFailed = Prefix + "UploadFailed";
    public const string DecodeFailed = Prefix + "DecodeFailed";
}