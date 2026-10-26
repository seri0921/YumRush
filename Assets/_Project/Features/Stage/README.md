# バイオームの開始時配置

GameシーンのStageにある`BiomeManager`で設定する。

- `Randomize On Start`がON：`Available Biomes`から重複なしでSlot数だけ選んで配置する。
- OFF：Editorで各SlotのContent配下に配置したBiomeをそのまま使う。
- この設定は開始時に一度だけ読み取る。Play中にチェックを変えても再配置しない。
- ランダム配置が元の順番と同じになる場合もある。

現在の候補はBiomeA〜Dの4種類で、4つのSlotへの割り当てをランダム化する。
6種類登録すると、その中から重複なしで4種類を選択する。

## 候補を追加する

1. 区画中央を原点とするBiome Prefabを作り、ルートに`BiomeContent`を追加する。
2. `Biome Id`に他の候補と重複しない固定IDを設定する。IDはPrefab名やHierarchy順とは独立している。
3. Stageの`BiomeManager > Available Biomes`へ、そのPrefabの`BiomeContent`を登録する。

Slotの`Content`配下には、OFF時に使うBiomeを1つ配置し、`Current Biome`へ割り当てる。
固定配置の背景・リスポーン参照は、従来どおりEditorで設定する。
`Main Camera`と`Respawn Manager`はシーン上のStageのBiomeManagerへ設定する。
カメラと区画範囲、区画間トリガーはSlot側・Stage側に残り、Biomeと一緒には交換しない。

## 責任分担と初期化

- `BiomeLogic`：Unityに依存せず、候補IDから配置順を決定する。候補不足・ID重複は設定エラーとする。
- `BiomeManager`：開始時設定を読み、Logicの結果をUnityへ反映する。
- `BiomeSlot`：配置位置・区画範囲・現在のBiomeを持つ。
- `BiomeContent`：背景とChunkのシーン参照を有効化前に接続する。

ランダム配置では、非アクティブな親の下で全候補の実体を生成し、参照設定を終えてから既存配置と交換する。
既存Biomeは直ちに無効化し、フレーム末に破棄する。新規Chunkは有効化時にSpawnPointを初期化する。
初期配置を決める段階では、Editorの候補一覧やPrefabアセットを変更しない。

今回は開始時の配置だけを扱う。プレイヤーの安全な開始位置の選択、区画間の到達可能性、
アイテム取得・ボス撃破による実行中の交換、交換時の状態引き継ぎは実装していない。

## 検証

EditModeの`Project.Stage.EditorTests`で、6候補から4つの選択・重複なし・入力保持・
同一シードでの再現・設定不正時の例外を検証する。
Play確認ではON/OFF、背景の区画範囲、カメラ追従、SpawnPoint生成とConsoleエラーを確認する。
