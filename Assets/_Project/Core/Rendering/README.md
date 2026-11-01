# スプライトのアウトライン

`Project/2D/Sprite Outline` は、透明度の境界に内側アウトラインを描くURP 2D用シェーダーです。
Tightメッシュ、画像のインポート設定、当たり判定を維持するため、描画メッシュを外側へ拡張しません。

## 調整

| マテリアル | 適用先 | 初期の太さ | 2Dライト |
| --- | --- | --- | --- |
| `Materials/CharacterOutlineLit.mat` | プレイヤー本体・攻撃画像、敵、弾、バンパー、アイテム | 1.5テクスチャpx | 有効 |
| `Materials/TerrainOutlineUnlit.mat` | 既存の足場5種類 | 0.75テクスチャpx | 無効（元の表示方式を維持） |

- `Outline Color` で線の色と濃さ、`Inner Outline Width` で太さを調整します。太さ0で無効になります。
- 太さは画面pxではなくテクスチャpxです。素材の解像度やカメラの拡縮で画面上の太さが変わります。
- `Outline Sprite Rectangle Edges` は画像範囲の外を透明として扱う設定です。足場では無効にし、スライスの接続部分に線が出るのを抑えています。画像範囲に接する不透明な地形の外端にも線は出ません。
- SpriteRendererの色・透明度を反映します。輪郭も2Dライトの影響を受けます。

## 新しいオブジェクトへの適用

1. SpriteRendererに対応する共有マテリアルを設定します。
2. 同じGameObjectに `SpriteOutlineView` を追加します。
3. アニメーション、左右反転、背景との境界、地形の接続部分をGame Viewで確認します。

`SpriteOutlineView` は画像範囲をMaterialPropertyBlockで渡します。アニメーションでSpriteが変わると範囲を更新し、スプライトシートの隣接画像を輪郭判定に混入させません。マテリアルの複製は行いません。
ゲームルールやPresenterは変更していません。

## 適用済みPrefab

以下はすべて `Assets/_Project/Features/` 以下です。入れ子Prefabには元Prefabの設定が継承されます。

- `Player/Prefabs/Player.prefab`
- `Enemy/Prefabs/Boss_Axolotl.prefab`
- `Enemy/Prefabs/Boss_Bat.prefab`
- `Enemy/Prefabs/Boss_Owl.prefab`
- `Enemy/Prefabs/Boss_Penguin.prefab`
- `Enemy/Prefabs/Minor_Axolotl.prefab`
- `Enemy/Prefabs/Minor_Owl.prefab`
- `Enemy/Prefabs/Minor_Penguin.prefab`
- `Enemy/Prefabs/EnemyNormal.prefab`
- `Enemy/Prefabs/Bumper.prefab`
- `Enemy/Prefabs/Bullet/BossBatBullet.prefab`
- `Enemy/Prefabs/Bullet/EnemyBullet.prefab`
- `Enemy/Prefabs/Bullet/MinorAxolotlBullet.prefab`
- `Enemy/Prefabs/Bullet/MinorBatBullet.prefab`
- `Items/Prefabs/ScoreItem.prefab`
- `Items/Prefabs/ScoreItemMagnet.prefab`
- `Stage/Prefabs/floor_ice.prefab`
- `Stage/Prefabs/floor_kusa.prefab`
- `Stage/Prefabs/floor_kusa02.prefab`
- `Stage/Prefabs/floor_rock.prefab`
- `Stage/Prefabs/floor_sand.prefab`

背景・UI・照準・移動エフェクト・パーティクルには適用していません。敵の弾は危険物として適用しています。

## 制限

- 内側の線なので、細い絵や小さい弾は濃く見える場合があります。必要に応じて専用マテリアルで太さを下げてください。
- 半透明の画像では、透明度の勾配にも線が現れます。
- 隣接ピクセルを8方向参照します。MaterialPropertyBlockも使用するため、大量表示時の描画負荷・バッチングは対象端末で確認してください。
- 全バイオーム・全戦闘パターン、実機での性能確認は別途必要です。
