using UnityEngine;
using UnityEngine.InputSystem; // 新しいInput Systemを使うために必要
using StarterAssets;

public class CursorUnlocker : MonoBehaviour
{
    private StarterAssetsInputs _inputs;

    void Start()
    {
        _inputs = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        // 新しいInput Systemでのキー入力判定
        // Keyboard.current は現在のキーボード、tabKey はTabキーを指す
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            // 現在の状態を反転させる（トグル方式）
            bool nextState = !_inputs.cursorLocked;
            SetCursorState(nextState);
        }
    }

    //cursorLocked と cursorInputForLook を切り替えることで、カーソル解除時にマウスを動かしてもカメラが勝手に回転しないように制御
    void SetCursorState(bool newState)
    {
        _inputs.cursorLocked = newState;
        _inputs.cursorInputForLook = newState;
        
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !newState;
    }
}