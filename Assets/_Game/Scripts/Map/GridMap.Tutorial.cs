using UnityEngine;

public partial class GridMap
{
    public TutorialFocusData GetTutorialFocus(TutorialFocusKind kind)
    {
        switch (kind)
        {
            case TutorialFocusKind.Player:
                return CreateFocus(player.WorldPosition,
                    player.DisplaySprite, player.DisplayColor);
            case TutorialFocusKind.Target:
                return TargetFocus();
            case TutorialFocusKind.Finish:
                return CreateFocus(finishPoint.WorldPosition,
                    finishPoint.DisplaySprite, finishPoint.DisplayColor);
            default:
                throw new UnityException("Loại mục tiêu tutorial không hợp lệ.");
        }
    }

    private TutorialFocusData TargetFocus()
    {
        foreach (TargetPoint target in targets.Values)
        {
            return CreateFocus(target.WorldPosition,
                target.DisplaySprite, target.DisplayColor);
        }
        throw new UnityException("Level tutorial cần có một cá mập con.");
    }

    private TutorialFocusData CreateFocus(Vector3 worldPosition,
        Sprite sprite, Color color)
    {
        Vector2 screenPosition = cameraController.WorldToScreen(worldPosition);
        return new TutorialFocusData(screenPosition, sprite, color);
    }
}
