# うにてぃの使いやすいスクリプト
うにてぃとは

# 各スクリプトの説明

- Multiplayer.cs

アタッチするだけでUDPマルチプレイが可能なスクリプト。
myPortに自分のポートをopponentPortには通信相手のポートを入力。
playerに自分のキャラクター、otherPlayerに相手のキャラクターを登録。
ParrelSyncなどを利用して2つのクライアントを起動した後に、コンポーネントのContextMenuからRegisterをそれぞれのクライアントで押す。
enableSmoothingを押すことで受信間のフレームで線形補完をする。

奇妙なスレッドの使い方をしているが、これはUnityAPIをサブスレッドから利用できないため。

- LazyLight.cs

カメラの向きをゆっくりと追うライト

- CreateDataBase.cs

エディタ拡張によりCSVからスクリプタブルオブジェクトを自動生成できる。

- TouchField.cs

スマホのジョイスティックを伴わない移動・視点操作に使える。コライダーが必要。
publicなメンバーのTouchDistで毎フレームの入力を参照できる。

- Localizer.cs

Unityのゲームを多言語、特に３つ以上の言語に対応させるときに使える。
Resource下に配置された各言語のファイルをenumのInGameTextと同じ順番だと想定して読み込むことでローカライズする。
テキストの区切りは改行、テキスト内の改行は'$'でエスケープする
実際にコンポーネントにする際はtargetTMPにTMP、targetInGameTextに対応するenumを同じ順番で入れる。
フォントの変更には未対応。TMPに代入するときに一緒に変える形で実装はできる。

- jp.txt

localizer.csのサンプルテキスト。Resource下に置く。

- MyCollision.cs

衝突判定用のスクリプト。銃弾など速い物体の衝突に用いる。
そのままではRigidBody必須だが、すこし変えれば無くても動く。
すり抜ける際は生成時にScanForward()を生成側から呼び出すこと。

- Network/UDP

UDP通信のサンプルスクリプト。
正直蛇足で、UdpClientのAPIを参照するほうが柔軟に作れる。
ちなみにスレッドで受信した内容をもとに処理をおこなうとUnity APIに怒られるので別のMonobehaviourのキューに保存し、Updateで処理を行うのが吉。

- Signal.cs

視界方向にオブジェクトがあるかを判断するスクリプト。敵AIの挙動のRaycast削減に使える。
xzしか考慮してないが、forwardとobject - positionの角度をVector3.Angle()で得ることで三次元にも適用可能。
(例えばそのままの場合y=1でもxzの向きさえ正しければy=1000のオブジェクトに反応する、しかし三次元にした場合y=1000の方向を見ないと反応しなくなる)
