# リポップの設定

## 既存Chunkの移行

1. Edit Modeで、アイテム・敵を含むシーン上のChunkを選択する。
2. `Tools > Respawn > Configure Selected Chunk` を実行する（Undo対応）。
3. Chunk内のScoreItem・EnemyBaseのPrefabインスタンスがSpawnPointに置き換わる。
   実体はPlay開始時に生成する。SpawnPointのTransformが生成位置・回転・スケールとなる。
4. 各SpawnPointのConfigに個別のRespawnConfigを設定する。
   初期の共有設定は個別10秒、自動Chunk再生成OFF（ON時60秒）。
   別の時間にしたい配置には、Create > Respawn > Individual Configで別アセットを作る。
5. ChunkRespawnControllerのConfigにChunkごとのChunkRespawnConfigを設定する。
6. シーンを保存する。移行メニューは既存Prefabアセットを変更しない。

対象はChunk直下または子孫のPrefabインスタンスのルートに置く。
位置・回転・スケール・名前・Active以外のオーバーライドは、先にPrefab Variantへ保存する。
入れ子のChunk、入れ子の生成対象には対応しない。選択Chunk自身を生成対象にはできない。
Chunk Prefabにする場合、シーンへ配置後にシーンのRespawnManager参照を割り当てる。

## 手動設定

シーンにRespawnManagerを配置し、ChunkにChunkRespawnControllerを追加する。
ManagerとConfigを割り当て、子に空のGameObjectとSpawnPointを配置する。
SpawnPointのPrefabとConfigを割り当てる。Prefabの実体を同時に配置すると二重になるため不要。
PrefabルートにScoreItemまたは対応するEnemyを置く。生成後にRespawnTargetが自動付与される。

## イベントによる補充

`chunk.RespawnAll()` または `manager.RespawnChunk(chunk)` を呼ぶ。
生存中の対象を残し、消滅した対象だけ補充する。個別タイマーは解除され、Chunk時計は再スタートする。
RespawnAllは引数なしなのでUnityEventからも呼び出せる。
特定のアイテム取得・ボス撃破イベント自体の実装は、それぞれの機能側で行う。

新しい敵・アイテムは、消滅が確定した時点で、ルートの任意コンポーネント
`RespawnTarget.NotifyConsumed()` を呼び、その後に非表示化・Destroyする。
ScoreItemとShootingEnemyには接続済み。
単なるDestroy、非アクティブ化、画面外移動は取得・撃破として扱わない。

## 時間とライフサイクル

- 個別時間は取得・撃破の通知から計測する。0秒は次回のManager更新で再生成。
- Chunk時間は有効化から計測し、一括補充のたびにリセットする。
- Time.deltaTimeを使うためtimeScale=0で停止する。
- Chunk無効中は個別時計が停止する。再有効化時、個別状態は維持しChunk時計はリセットする。
- 無効なSpawnPointは補充対象外。初期実体はChunkの初期化時に子として生成される。
- Chunk破棄時は登録解除され、子の実体も破棄される。遅延非同期処理は残らない。
- 再生成は新規Instantiateなので取得済み・撃破済み状態は引き継がない。
- 実行中のConfig編集は未対応。次のChunk有効化／新規SpawnPoint初期化時に反映する。
- 地形の作り直し、ObjectPool、セーブへの状態保存、敵の安全距離判定は対象外。

## 検証

Unity Test RunnerのEditModeで`Project.Respawn.EditorTests`を実行する。
純粋な時間管理、境界値、不正遷移、共有設定での独立性、重複生成防止、Chunk登録解除を検証する。
実際のゲームでは取得・撃破後の復活、加点、当たり判定、ポーズ、Chunk切り替えを確認する。
