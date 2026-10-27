# 時間イベント

## 設定手順

1. ProjectのCreate > Stage Events > Timed EventからSOを作成する。
   - Type：BonusTime（スコアアイテム大量出現）またはBoss。
   - Start Seconds：ゲーム開始からの発生秒。0秒も指定できる。
   - Duration Seconds：継続秒。終了はStart Seconds + Duration Seconds。
2. 各Biome Prefabの配下にイベント専用の空オブジェクトを作り、EventChunkを追加する。
   Typeを設定し、**GameObjectを非アクティブにして保存する**。
3. EventChunkの子にScoreItem Prefabを必要な数だけ、またはボスPrefabを直接配置する。
   通常のChunkRespawnController、SpawnPoint、RespawnTargetは使わない。
   EventChunk同士は入れ子にしない。親はバイオームとともに有効になる場所にする。
4. シーンの有効なオブジェクトにStageEventPresenterを追加する。
   Game Manager、PlayerのTransform、全BiomeSlot、イベントSOの一覧を割り当てる。
   一覧は時刻順でなくてもよい。同じ時刻では一覧の先頭を優先する。
5. ランダム配置OFFで使うシーン上のBiomeにもEventChunkを設定する。
   ランダム配置ON時はBiomeManagerが生成したCurrentBiomeから配置を取得する。

シーン・既存Prefabへの自動配置や、サンプル時刻の登録は行っていない。

## 再利用するボスChunkの差し替え設定

1. バイオームごとに、地形・Collider・ボスをまとめたボスChunk Prefabを作成する。
   EventChunk、ChunkRespawnController、SpawnPoint、RespawnTargetは含めない。
2. Biome配下に空の非アクティブなオブジェクトを置き、EventChunk（Type = Boss）と
   BossChunkReplacementを付ける。このオブジェクトは差し替え位置の基準となる。
   通常Chunkの子や、その通常Chunkを含む親には置かない。
3. BossChunkReplacementのNormal Chunksに、無効化する通常Chunkのルートを登録する。
   同じBiome内の独立したルートを指定し、重複や親子の重なりを含めない。
   Boss Chunk PrefabにそのBiome用のPrefabを割り当てる。
4. ボスPrefabはアンカーの子として生成される。Prefabルートのローカル座標・回転・スケールと
   アンカーのTransformを合わせて配置する。通常Chunkを複数指定しても、生成するPrefabは一つ。
5. TimedEventConfig（Boss）を開催時刻ごとにPresenterへ登録する。
   繰り返し開催は複数の設定エントリーで指定する。自動的な周期開催は行わない。

### ボスChunkの寿命

- 開始時：通常ChunkのactiveSelfを保存して無効化し、ボスPrefabを新規生成する。
- 開催中：プレイヤーが別Biomeにいても通常Chunkには戻さない。
- 終了時：プレイヤーが対象Biomeにいる場合は復帰待ちとし、ボスChunkを維持する。
  すでに別Biomeにいれば、その場で通常Chunkへ戻す。
- 復帰待ち：別Biomeにいることを確認してからボスChunkを撤去し、通常Chunkの元の有効状態を復元する。
  範囲外・境界の重複などで所属不明の場合は戻さない。時計停止中も毎フレーム移動を確認する。
- 復帰するまで同じBiomeは全種別の新規イベント候補から外れる。開始時に候補がなければ従来どおりスキップする。
- 次回開催ではPrefabから再生成するため、破壊されたボスも再配置される。
  アンカーと設定を再利用し、終了直後のボス個体を使い回さない。
- 復帰待ちでもボス・残存オブジェクトの動作は継続する。ボス行動の停止や無敵化は追加しない。
- 通常Chunkの既存オブジェクトは保持する。ChunkRespawnControllerは再有効化時に
  Chunkのリスポーン時計をリセットする（既存実装）。
- ボスChunk外に生成された弾・エフェクトや、static等の外部状態は再生成・撤去の対象外。

## 開催ルール

- 各一覧エントリーは一度だけ判定する。同じSOを複数登録すると、それぞれ別の開催になる。
- 開始時点のPlayer座標を、各SlotのCameraBounds（PolygonCollider2D）で判定する。
  範囲外・複数範囲内で現在地が一意に定まらない場合はスキップする。
  Boundsは有効なColliderとして設定する。
- 現在地以外、イベント未開催かつボスChunkの復帰待ちでなく、対象種別の利用可能ChunkがあるSlotから均等に抽選する。
  複数Chunkがあっても、そのSlotの抽選確率は増えない。
- 候補なしの場合はログを出してスキップし、再試行しない。
- 異なるSlotでの同時開催は可能。同じSlotでは種別を問わず同時開催しない。
- 開始後にPlayerが移動しても開催先は変わらない。
- 取得したアイテムや撃破したボスは補充しない。早期に全滅しても場所の予約は時間まで続く。
- BossChunkReplacementを持たない既存のイベントChunkは、終了時に直ちに無効化して破棄する。
  **これらのChunkは使い切り**。同じBiomeで同じ種別を複数回開催したい場合は、
  必要数の独立したEventChunkを事前配置する。未使用の配置がなければ候補から外れる。
- ボスがChunk外へ発射した弾は消去対象に含まない。
- 同時刻の終了と開始は、終了を先に処理する。
- フレームの遅延などで終了時刻まで完全に通り過ぎた未開催イベントはスキップする。
  開始を少し越えて検出しても終了時刻は延長しない。

## 時間・ライフサイクル

GameManager.TimeAdvancedの経過秒を共有し、独自タイマーは持たない。
PauseTimerによる時計停止中は継続時間も進まないが、ボス行動などの全体停止は別の責任。
GameOver・Presenter無効化時も実行中のイベントを終了させる。
ボスの地形はその場合も安全な復帰条件を守り、プレイヤーが対象Biome内なら保持する。
Presenter無効化中は移動を監視せず、再有効化時に復帰を判定する。
シーン終了時にはボスの生成物もアンカーとともに破棄される。
Presenterの無効化中に開始と終了を通過したイベントは再有効化時にスキップする。
実行中のBiome交換や、同じシーンでのセッション再スタートは今回の対象外。

## 責任分担と検証

- TimedEventConfig：設定アセット。実行時状態を保存しない。
- EventTiming：Unityに依存しない設定のスナップショット。
- EventScheduleLogic：時間判定・開催場所の抽選と予約。Unityへの依存なし。
- StageEventPresenter：時間通知の購読と解除、現在地判定、シーン配置の接続。
- EventChunk：イベントの開始・終了を、使い切りコンテンツまたはボス差し替えへ接続する。
- BossChunkStateLogic：ボス開催中・復帰待ち・再利用可能の状態と復帰条件。Unityへの依存なし。
- BossChunkReplacement：通常Chunkの有効状態の保存・復元とボスPrefabの生成・撤去。

Project.Events.EditorTestsで復帰条件（開催中、終了後の同じBiome・所属不明・別Biome）、
復帰待ちの再開始拒否と復帰後の再利用を検証する。
同アセンブリのUnityTestはEnterPlayModeで独立したテストオブジェクトと一時Prefabを使い、
通常Chunkの有効状態の復元、復帰待ち、撃破されたボスの次回再生成を検証する。
既存のGameシーンに実際のボス・大量アイテムを配置したプレイ確認は別途必要。

```text
uloop run-tests --test-mode EditMode --filter-type assembly --filter-value Project.Events.EditorTests --unsaved-changes fail
uloop verify-project
```
