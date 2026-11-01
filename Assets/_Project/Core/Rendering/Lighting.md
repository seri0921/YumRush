# ライティングの調整

## 構成

`Game.unity` の環境光とVolume、`Player.prefab` の2つの局所ライトで調整しています。
影、ノーマルマップ、新規の実行時C#は追加していません。ゲームルールや物理処理は変更していません。
背景はライトから独立させ、地形とキャラクターには距離による明暗をつけています。
局所ライトはプレイヤーの子なので一緒に移動します。Canopy Fill Lightは実際の太陽ではなく、周辺の視認性を支える演出光です。

| 場所 | 設定 | 初期値 |
| --- | --- | --- |
| Game / Global Light 2D | Intensity / Color | 0.62 / RGB (0.80, 0.86, 1.00) |
| Player / Player Aura Light | Intensity / Inner・Outer Radius | 0.85 / 0.4・5 |
| Player / Canopy Fill Light | Intensity / Inner・Outer Radius | 0.45 / 2・14 |
| Player / Canopy Fill Light | Local Position / Color | (-3, 5, 0) / RGB (1, 0.87, 0.67) |
| ForestLightingProfile | Bloom Threshold / Intensity / Scatter | 1.05 / 0.28 / 0.6 |
| ForestLightingProfile | Saturation / Contrast | 8 / 8 |
| ForestLightingProfile | Tonemapping | Neutral |
| ForestLightingProfile | Vignette Intensity / Smoothness | 0.16 / 0.65 |

## 調整する順序

1. Game Viewで確認しながらGlobal LightのIntensityを調整します。下げるとプレイヤーから離れた地形・敵が暗くなります。背景の色は維持されます。
2. Player Aura LightのOuter Radiusで近接光の範囲、Intensityで中心の明るさを調整します。上げすぎるとキャラクターが白く見えます。
3. Canopy Fill Lightの位置・色・Intensityで広い範囲の色温度と明暗を調整します。光の範囲が広すぎると平坦に戻るので、Global Lightとの合計を見て調整します。
4. ForestLightingProfileのBloomを調整します。Thresholdを下げると通常の明るい背景までにじみます。Intensityは少しずつ上げてください。背景の一部分だけを発光させるマスクは今回追加していません。
5. 最後にSaturation・Contrastを調整します。明るい背景と暗い背景の両方で敵・弾の視認性を確認します。

背景4種のBackgroundAutoと同じオブジェクトのSpriteRendererには`BackgroundUnlit.mat`を設定しています。背景は色調整・Vignetteなどのポスト処理の影響は受けます。
足場5種のSpriteRendererには`TerrainOutlineLit.mat`を設定しています。元の`TerrainOutlineUnlit.mat`を複製し、Use 2D Lightingだけを有効にしたものです。元の輪郭設定を引き継ぎます。
新しく作った足場にもこのLitマテリアルを設定してください。シーンや入れ子Prefabでマテリアルを個別Overrideした箇所は、設定が継承されないことがあります。

## 確認範囲と制限

Play Modeで4背景の画面、局所ライトのプレイヤー追従、地形のLitマテリアル、Console Error 0件を確認しています。
確認位置への移動は実行時だけの操作で、初期位置には保存していません。Play Modeは終了しています。
実機性能・全チャンク・全戦闘パターンは未検証です。追加した2灯の描画負荷は対象端末で確認してください。
表面の向きに応じた陰影や遮蔽物による影はありません。今回の奥行きは素材に描かれた明暗と、環境光・局所光の差による表現です。

## 元の見え方へ戻す場合

Global Lightを白・Intensity 1へ戻し、Playerの追加ライト2つとForest Lighting Volumeを無効にします。
足場をTerrainOutlineUnlitへ、背景を元のSprite-Lit-Defaultへ戻すと、今回追加した明暗の変化を外せます。
カメラのPost Processingも今回有効にしています。他のVolumeを使う場合は必要に応じて維持してください。
