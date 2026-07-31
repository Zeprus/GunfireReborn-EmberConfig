namespace EmberConfig.PrefabDataGen.Models;

internal readonly record struct RectData(
    float AnchorMinX,
    float AnchorMinY,
    float AnchorMaxX,
    float AnchorMaxY,
    float SizeDeltaX,
    float SizeDeltaY,
    float AnchoredPositionX,
    float AnchoredPositionY,
    float PivotX,
    float PivotY);
