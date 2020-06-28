using UnityEngine;

public class PlayerAnimationEventManager : MonoBehaviour
{
    public void OnPickupEvent()
    {
        CardBuilder.instance.OnPickupEvent();
    }

    public void OnStepAwayEvent()
    {
        CardBuilder.instance.OnStepAwayEvent();
    }

    public void OnDropEvent()
    {
        CardBuilder.instance.OnDropEvent();
    }
}
