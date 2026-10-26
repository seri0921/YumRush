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

## 開催ルール

- 各一覧エントリーは一度だけ判定する。同じSOを複数登録すると、それぞれ別の開催になる。
- 開始時点のPlayer座標を、各SlotのCameraBounds（PolygonCollider2D）で判定する。
  範囲外・複数範囲内で現在地が一意に定まらない場合はスキップする。
  Boundsは有効なColliderとして設定する。
- 現在地以外、イベント未開催、対象種別の未使用ChunkがあるSlotから均等に抽選する。
  複数Chunkがあっても、そのSlotの抽選確率は増えない。
- 候補なしの場合はログを出してスキップし、再試行しない。
- 異なるSlotでの同時開催は可能。同じSlotでは種別を問わず同時開催しない。
- 開始後にPlayerが移動しても開催先は変わらない。
- 取得したアイテムや撃破したボスは補充しない。早期に全滅しても場所の予約は時間まで続く。
- 終了時にはChunkを直ちに無効化して、そのGameObjectごと破棄する。
  **各Chunkは使い切り**。同じBiomeで同じ種別を複数回開催したい場合は、
  必要数の独立したEventChunkを事前配置する。未使用の配置がなければ候補から外れる。
- ボスがChunk外へ発射した弾は消去対象に含まない。
- 同時刻の終了と開始は、終了を先に処理する。
- フレームの遅延などで終了時刻まで完全に通り過ぎた未開催イベントはスキップする。
  開始を少し越えて検出しても終了時刻は延長しない。

## 時間・ライフサイクル

GameManager.TimeAdvancedの経過秒を共有し、独自タイマーは持たない。
PauseTimerによる時計停止中は継続時間も進まないが、ボス行動などの全体停止は別の責任。
GameOver・Presenter無効化・シーン終了時も実行中のChunkを終了させる。
Presenterの無効化中に開始と終了を通過したイベントは再有効化時にスキップする。
実行中のBiome交換や、同じシーンでのセッション再スタートは今回の対象外。

## 責任分担と検証

- TimedEventConfig：設定アセット。実行時状態を保存しない。
- EventTiming：Unityに依存しない設定のスナップショット。
- EventScheduleLogic：時間判定・開催場所の抽選と予約。Unityへの依存なし。
- StageEventPresenter：時間通知の購読と解除、現在地判定、シーン配置の接続。
- EventChunk：配置したUnityオブジェクトの有効化と終了時の破棄。

Project.Events.EditorTestsで、境界時刻、一度だけの発生、場所の排他、スキップ、
種別別の候補、時刻順の処理、場所固定、不正設定を検証する。
Project.Events.PlayModeTestsでは独立したテスト用オブジェクトを生成し、時間通知からの開始、
現在地の除外、期限での子オブジェクト破棄、Presenter停止時の終了を検証する。
既存のGameシーンに実際のボス・大量アイテムを配置したプレイ確認は別途必要。

```text
uloop run-tests --test-mode EditMode --filter-type assembly --filter-value Project.Events.EditorTests --unsaved-changes fail
uloop run-tests --test-mode PlayMode --filter-type assembly --filter-value Project.Events.PlayModeTests --unsaved-changes fail
uloop verify-project
```
