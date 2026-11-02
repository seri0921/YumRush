# ライティングの状態

## 2026-11-02 リセット

ライティングを組み直すため、基盤導入コミット `54167ad` より前の環境光・ポスト処理・背景マテリアルを基準に戻しました。ライティング以外の作業中の変更は保持しています。

| 対象 | 現在の状態 |
| --- | --- |
| Game / Global Light 2D | 白（RGB 1, 1, 1）、Intensity 1 |
| Player / Player Aura Light | 無効（Prefab本体とGameシーンのOverride） |
| Player / Canopy Fill Light | 無効（Prefab本体とGameシーンのOverride） |
| Game / Forest Lighting Volume | 無効 |
| Main Camera / Post Processing | 無効 |
| 背景 | Stage.prefabとBiomeA〜Dの計8参照を元のSprite-Lit-Defaultへ復帰 |
| 足場 | TerrainOutlineLitの参照はリセット時点ですでになく、現在の設定を保持 |

追加したライト・Volume・ForestLightingProfile・マテリアルのアセットは、後で組み直せるよう削除せず残しています。無効なライトやProfile内部の調整値は初期化していません。再度有効にすると保存されている値が適用されます。

## 変更ファイルと責任

- `Assets/_Project/Scenes/Game.unity`：環境光の復帰、追加ライトとVolume・カメラのポスト処理の無効化。
- `Assets/_Project/Features/Player/Prefabs/Player.prefab`：追加ライト2灯の無効化。
- `Assets/_Project/Features/Stage/Prefabs/Stage.prefab`：背景4参照を復帰。
- `Assets/_Project/Features/Stage/Prefabs/Biomes/BiomeA.prefab`〜`BiomeD.prefab`：各背景1参照を復帰。

表示設定だけの変更です。Logic / Presenter / Viewのコード・責任分担、ゲームルール、物理、描画順は変更していません。

## 検証

- 保存したGameシーンをUnityで再読み込みし、環境光が白・Intensity 1、追加ライト2灯とForest Lighting Volumeが無効、カメラのPost Processingが無効であることを確認。
- Play Modeの雪背景でGame Viewを撮影し、プレイヤー・敵・足場・アイテムが描画されることを目視確認。
- Play Modeは終了済み。
- 検証後のConsoleにはLakeのRespawnManager / ChunkRespawnConfig不足のエラーが3件ありました。以前から確認されている別件で、今回のリセット対象外です。
- 全背景・全戦闘パターン・実機性能は未検証。C#変更はないためコンパイルや単体テストは実施していません。

画面記録：`.uloop/outputs/Screenshots/Rendering_20261102_152632_731.png`

## 現在：プレイヤー追従ライト（2026-11-02）

リセット状態を基準に、Player Aura Lightのみ再度有効にしました。上記のリセット表はリセット時点の記録です。

| 設定 | 現在値 |
| --- | --- |
| Player Aura Light | 有効、Playerの子として追従 |
| Color / Intensity | RGB (1, 0.98, 0.94) / 0.3 |
| Inner / Outer Radius | 0.5 / 6（Unity単位） |
| Falloff Intensity | 0.7 |
| Global Light | 白・Intensity 1を維持 |
| Canopy Fill Light / Forest Lighting Volume / Post Processing | 無効を維持 |

### 変更内容・責任

- `Player.prefab`：Player Aura Lightの有効化、色・光量・半径を設定。
- `Game.unity`：Auraの無効化、光量2、半径20/30、Falloff 1の旧Overrideを削除し、Prefabの設定を継承。
- `floor_ice.prefab`、`floor_kusa.prefab`、`floor_kusa02.prefab`、`floor_rock.prefab`、`floor_sand.prefab`：各3か所、計15個のSpriteRendererを既存のTerrainOutlineLitへ変更。輪郭幅は既存マテリアルの0を維持。
- 新規の実行時コード・アセットは追加していません。表示設定に限定し、Logic / Presenter / Viewの責任分担は維持しています。

### 検証・制限

- Unity再読み込み後、Auraの光量0.3、半径0.5/6、色と親がPlayerであることを確認。
- Play Modeの雪背景で有効・無効を比較し、近くの足場の明るさがわずかに変わることを目視確認。環境光1を維持しているため効果は控えめです。
- 実行時のみ親をX方向へ1移動して戻し、ライトも同じ量を移動することを確認。
- 実行時の153個のSpriteRendererがTerrainOutlineLitを参照し、UseLightingが1であることを確認。
- 比較時のライト無効化・移動は保存していません。Play Modeは終了済み。
- Consoleには既存のLakeのRespawnManager / ChunkRespawnConfig不足エラー3件が残っています。
- 全背景・全戦闘・実機性能は未検証。C#変更はないためコンパイルや単体テストは実施していません。

画面記録（有効）：`.uloop/outputs/Screenshots/Rendering_20261102_154203_137.png`
画面記録（無効）：`.uloop/outputs/Screenshots/Rendering_20261102_154227_898.png`

## 現在：控えめなポストエフェクト（2026-11-02）

プレイヤー追従ライトに合わせ、GameのForest Lighting VolumeとMain CameraのPost Processingを有効化しました。先の「無効」の記述は過去の作業時点の記録です。

| エフェクト | 設定 |
| --- | --- |
| Bloom | Threshold 1.1 / Intensity 0.15 / Scatter 0.5 |
| Tonemapping | Neutral |
| Color Adjustments | Contrast +5 / Saturation -5 |
| Vignette | Intensity 0.1 / Smoothness 0.6 |

変更ファイルはGame.unity（Volume・カメラ有効化）、ForestLightingProfile.asset（上記の効果設定）、本書（調整記録）の3つです。表示設定に限定し、Logic / Presenter / Viewのコードや責任分担は変更していません。

HDRはURPとカメラの両方で有効、Renderer2DのPostProcessDataが設定済みで、VolumeのDefaultレイヤーがカメラのVolume Layer Maskに含まれることを確認しました。Unity上で効果の設定値と有効状態を確認し、雪背景のPlay Modeでポスト処理の有無を撮影して比較しました。画面端の軽い減光と色味の変化を確認し、プレイヤー・敵・床・アイテムの判別を維持しています。明確な白飛びや強いにじみは比較画面では見られませんでした。

開始時にGameシーンに未保存編集があったため、シーンの再読み込み・一括保存はしていません。ディスクのポスト処理2項目だけを更新し、開いているシーンにも同じ設定を反映して、既存の未保存編集を保持しました。Play Modeは終了済みです。

Consoleには既存のLakeのRespawnManager / ChunkRespawnConfig不足エラー3件が残っています。全背景・全戦闘・実機性能は未検証です。C#変更はないためコンパイルや単体テストは実施していません。

画面記録（有効）：`.uloop/outputs/Screenshots/Rendering_20261102_161029_114.png`
画面記録（無効）：`.uloop/outputs/Screenshots/Rendering_20261102_161044_942.png`

最終確認ではPost Processingが有効、シーンは保存済み状態でした。作業開始時のディスクとの差分には、カメラ位置の微調整とAuraのFalloff 0.223・内側半径3・外側半径8も含まれていました。これらは今回のポスト処理変更で設定した値ではなく、既存の編集として保持しています。

## 現在：ライト境界とBloomの調整（2026-11-02）

広い均一照明域と低いBloom Thresholdを見直し、ライトの減衰域を広げて白い部分の階調を保つ設定にしました。以下が最新値です。

| 対象 | 調整前 | 現在 |
| --- | --- | --- |
| Player Aura / Intensity | 0.39（シーンOverride） | 0.25 |
| Inner / Outer Radius | 11.33 / 20（シーンOverride） | 1 / 20 |
| Falloff Intensity | 0.494（シーンOverride） | 0.5 |
| Bloom Threshold | 0.52 | 1.1 |
| Bloom Intensity | 1.26 | 0.8 |
| Bloom Scatter | 0.5 | 0.8 |
| Tonemapping | Neutral | Neutralを維持 |

### ファイルと設計判断

- `Player.prefab`：Auraの光量・半径・減衰を上記へ設定。
- `Game.unity`：Auraの光量・半径・減衰の旧Overrideを削除してPrefabへ統一。他のOverrideは保持。
- `ForestLightingProfile.asset`：Bloomの3値を更新。Tone・色調補正・Vignetteは維持。
- `Lighting.md`：調整値と比較結果を記録。

新規コード・依存・アセットは追加せず、表示設定に限定しました。Logic / Presenter / Viewの責任分担は変更していません。

### 検証と制限

Unity読み込み後にライトの光量0.25、半径1/20、Falloff 0.5を確認。Play Modeのオアシス背景でNeutralとACESを比較しました。ACESでは敵と背景の暗部が沈んで見えるため、Neutralを採用しました。Neutralではプレイヤーの白い部分の線が判別でき、強い円形の境界は目立ちませんでした。元設定との同一フレーム比較や全背景での確認は行っていないため、全状況での解消を保証する検証ではありません。

ACESへの変更は実行時だけで、保存していません。Play Modeは終了済み。Consoleには既存のLakeのRespawnManager / ChunkRespawnConfig不足エラー3件が残っています。全戦闘・実機性能は未検証。C#変更はないためコンパイル・単体テストは実施していません。

比較画像：Neutral `.uloop/outputs/Screenshots/Rendering_20261102_165304_736.png` / ACES `.uloop/outputs/Screenshots/Rendering_20261102_165322_059.png`
