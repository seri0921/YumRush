# 宝箱システム

Gameシーンの`TreasureSession`が、開始時に一度だけ宝箱1個と鍵を配置します。
Sandbox/TreasureSystemとは独立しています。バイオームは固定配置です。

## Inspector設定

- `TreasureSession > Required Keys`：合計鍵数。最低4。初期値4。
- `Biome Key Prefabs`：0=BiomeA/Owl、1=BiomeB/Penguin、2=BiomeC/Bat、3=BiomeD/Axolotl。
- `Chest Prefab` / `Reward Prefab`：宝箱と報酬のPrefab。
- `Spawn Points`：抽選候補の全参照。候補を追加したら、この配列にも登録してください。
- `TreasureSpawnPoint > Biome Index`：所属バイオーム（0～3）。
- `Key Weight`：鍵の抽選重み。0は鍵の候補から除外。1に対して2は各抽選で2倍の重み。
- `Can Spawn Chest`：宝箱の候補に含めるか。宝箱だけの候補はKey Weightを0にします。
- `Configs/TreasureAppleScore.asset > Points`：報酬リンゴの点数。初期値1000。

各バイオームに4か所、合計16か所の候補を配置済みです。
初期地点は既存リンゴの配置位置を参考にしており、通常のSpawnPointとは独立しています。
Hierarchyの`TreasureSession/BiomeA_TreasurePoints`等から位置を調整できます。
Scene Viewの黄色の球は宝箱候補、水色の球は鍵のみの候補です。
必要数を増やすときは十分な候補を追加してください。現在の候補数では最大15個です。

## 抽選と進行

1. 鍵の必要数・各バイオーム最低1個を満たせる宝箱候補から均等抽選。
2. 宝箱の地点を除き、各バイオームから重み付きで鍵を1個ずつ選択。
3. 残りの鍵をステージ全体の候補から重み付きで選択。重複はなし。

同じ抽選段階では重みの比率が選ばれやすさを決めます。
最低1個の保証があるため、ステージ全体での最終的な配置確率は単純な重みの比率とは異なります。
候補不足・参照不足・同一位置・不正な重みは設定エラーとして扱い、必要数を黙って減らしません。

鍵はホーミング攻撃で取得します。全鍵取得後に宝箱が攻撃対象になり、攻撃すると開きます。
リンゴは既存の磁石・取得処理を使用します。宝箱と報酬は一度しか開封・取得できません。
宝箱は`Sprites/Chest_01.png`の1枚絵を使用します。画像の比率を保ち、幅2のサイズで表示します。
鍵不足時は暗く、全鍵取得後は通常の色で表示し、開封すると宝箱の画像が消えてリンゴが出現します。
開封状態と生成位置の参照を保持するため、宝箱本体は破棄せずSpriteRendererだけを非表示にします。

## 状態の寿命

取得数・開封状態はTreasureSessionが所有し、通常の敵・アイテム用RespawnManagerには登録しません。
プレイヤーだけの復帰、通常Chunkの補充、TreasureSessionの無効化・再有効化では状態を維持します。
取得済みの鍵・リンゴは復活せず、未取得のリンゴは残ります。
Gameシーンを新規ロードするとリセットし、配置を再抽選します。
現時点のPlayerPresenterにシーンを再読み込みするミス復帰処理はありません。
将来、ミス復帰のためにGameシーンを再ロードする仕様を追加する場合は、状態引き継ぎの接続が必要です。
アプリ終了後の保存は行いません。

## 責任と検証

- `TreasurePlacementLogic`：Unity非依存の配置条件検証、宝箱抽選、鍵の重み付き非復元抽選。
- `TreasureProgressLogic`：鍵の重複取得防止、解錠・開封状態。
- `TreasureSessionPresenter`：配置設定をLogicへ渡し、生成と進行を接続。
- `TreasureKeyPresenter` / `TreasureChestPresenter`：既存IEnemyによる攻撃受付。
- `TreasureChestView`：解錠と開封の表示のみ。
- `TreasureSpawnPoint`：配置位置、バイオーム、重み、宝箱候補の設定。

EditModeテスト：Project.Treasure.EditorTests。
Play確認：4種の鍵生成、配置重複なし、解錠前の開封拒否、ホーミング対象探索・攻撃適用、
重複取得防止、全鍵で解錠、報酬1個、1000点加点、無効化・再有効化による状態維持。
全候補地点の床・壁との重なりも2D物理で検査しています。
手動操作による全地点への到達性や、未実装のミス復帰フロー全体は検証対象外です。

## 鍵・宝箱の方向表示UI

Gameシーンの`TreasureDirectionUI`が、プレイヤーから2D直線距離で最も近い未取得の鍵1個を案内します。
その鍵の中心がカメラの画面内にある間は非表示です。画面外の別の鍵には切り替えません。
取得後は次に近い未取得の鍵へ切り替え、全鍵取得後は宝箱を案内します。
宝箱も画面内では非表示とし、開封すると案内を終了します。
同距離の鍵は生成順で先のものを選びます。道のり・障害物による最短経路は計算しません。

- `TreasureDirectionPresenter`：Session、Player、Camera、Viewの参照を設定します。
- `TreasureDirectionView > Arrow Size`：矢印のサイズ。初期値48（1920×1080基準）。
- `Edge Padding`：画面端と矢印の間の余白。初期値24。回転時の見切れも考慮します。
- `Arrow Color`：矢印の色。初期値は白。
- `Arrow`配下のImage：指定の`Features/UI/Sprites/EventDirectionArrow.png`を使用します。
- CanvasScaler：1920×1080基準、縦横比を均等に考慮。描画順は19で、開始・終了カウントダウンや画面遷移より手前に出ません。

Prefabは`Prefabs/TreasureDirectionUI.prefab`です。別シーンに配置した場合はPresenterのシーン参照を設定してください。
既存イベント方向UIとは独立し、クリックや入力を遮りません。
プレイヤーを別GameObjectとして再生成する復帰処理を将来追加する場合は、Player参照も再接続してください。

`TreasureDirectionLogic`は最寄りの候補選択と画面端の座標・角度を純粋C#で計算します。
PresenterがUnityの座標・状態を渡し、Viewは矢印の位置・回転・表示を反映します。
Sessionは生成済みの鍵と宝箱の読み取り用参照を公開し、UIを直接参照しません。

追加テストは最寄り選択、同距離、対象なし、画面内、上下左右、縦長・横長、背面、小さい画面を検証します。
既存テストを含めEditModeテスト18件が成功しました。
Play Modeでは検証用に座標を制御して、1個のみの表示、画面内で別鍵へ切り替えないこと、
取得後の対象変更、全鍵取得後の宝箱案内、画面内の宝箱の非表示、開封後の終了を確認しました。
Game描画のキャプチャでも画面端の矢印を確認しています。キーボード操作による全ステージ走破は行っていません。

## 鍵Prefabのサイズ調整

`Prefabs/TreasureKey_Owl.prefab`、`TreasureKey_Penguin.prefab`、`TreasureKey_Bat.prefab`、`TreasureKey_Axolotl.prefab`は、当たり判定と見た目を個別に調整できます。

- 当たり判定：Prefabのルートにある`CircleCollider2D > Radius`を変更します。判定位置は`Offset`で調整します。
- 見た目：子オブジェクト`Visual > Transform > Scale`を変更します。初期値は各軸1で、2にすると見た目だけが2倍になります。
- ルートの`Transform > Scale`は当たり判定と見た目の両方に影響します。個別調整には上記の項目を使用してください。

Colliderと`TreasureKeyPresenter`は同じルートに置き、ホーミング攻撃による取得の接続を維持しています。
