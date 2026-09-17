namespace Genora.MultiTenancy.Enums.Hlg;

/// <summary>
/// Loại game — cấu hình động (quyết định nghiệp vụ BD-1).
/// Map contract frontend:
///  Quiz = "quiz", PictureToWord = "Picture-to-Word Puzzle",
///  KingOfVietnamese = "King of Vietnamese", SpinWheel = "Spin Wheel - Lucky Wheel",
///  TileFlip = "Tile Flip / Reveal the Image".
/// </summary>
public enum HlgGameType : byte
{
    Quiz = 1,
    PictureToWord = 2,
    KingOfVietnamese = 3,
    SpinWheel = 4,
    TileFlip = 5
}
