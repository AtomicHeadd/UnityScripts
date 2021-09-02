# うにてぃの使いやすいスクリプト
うにてぃとは

# 各スクリプトの説明

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
