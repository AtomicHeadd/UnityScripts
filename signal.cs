//レーダーのアニメーションにDotweenを使っています
using DG.Tweening;

[SerializeField] Camera playerCamera;
[SerializeField] GameObject nextEvenetObject;
[SerializeField] GameObject Signal_UI;
private Tweener currentTweener_signalUI;
private const int signalDegree = 45;
private int currentSignalPower = 0;

void Update(){
  Vector2 sub = new Vector2(nextEvenetObject.transform.position.x - playerCamera.transform.position.x, nextEvenetObject.transform.position.z - playerCamera.transform.position.z);
  float angle = Vector2.Angle(new Vector2(playerCamera.transform.forward.x, playerCamera.transform.forward.z), sub);
  int thisFlameSignalPower = 0;
  //print(angle);
  if (angle > -signalDegree && angle < signalDegree)
  {
    print("視界に入ってるよ");
    float distanse = (nextEvenetObject.transform.position - player.transform.position).magnitude;
    if (distanse <= 5) thisFlameSignalPower = 3;
    else if (distanse <= 20) thisFlameSignalPower = 2;
    else thisFlameSignalPower = 1;
  }
  else { print("視界に入ってないよ"); }
  //シグナルの強さが異なる場合更新、アニメーションを再生
  if (currentSignalPower != thisFlameSignalPower)
  {
    currentTweener_signalUI.Kill();
    currentSignalPower = thisFlameSignalPower;
    Signal_UI.transform.localScale = new Vector3(1, 1, 1);
    if (currentSignalPower != 0) currentTweener_signalUI = Signal_UI.transform.DOScale(0.9f, 1.0f / (float)currentSignalPower).SetLoops(-1, LoopType.Restart);
  }
}
