using UnityEngine;

public class Rock : InteractableOb
{
    public override void Interact(string toolTag)
    {
        if (toolTag != "Pickaxe") // 곡괭이가 아닐 경우 무시
        {
            Debug.Log("이 오브젝트는 곡괭이로만 상호작용할 수 있습니다!");
            return;
        }

        base.Interact(toolTag); // 부모 클래스의 Interact 호출
    }
}
