# YumRush

YumRush は Unity 製の 2D アクションゲームです。制限時間内にステージを移動し、アイテムの回収や敵の撃破でスコアを稼ぎます。タイトルからゲーム本編に入り、プレイ終了後にリザルトでスコアを確認できます。

## セットアップと初回プレイ

1. Git と Unity Hub を用意し、Unity Editor **6000.3.9f1** をインストールします。使用するバージョンは [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt) で確認できます。
2. リポジトリを取得します。

   ```sh
   git clone https://github.com/seri0921/CAPCOM-IPUT-2026.git
   cd CAPCOM-IPUT-2026
   ```

3. Unity Hub の「Add / 追加」から、`Assets/`、`Packages/`、`ProjectSettings/` があるリポジトリのルートを選びます。初回はアセットのインポートとパッケージの取得が完了するまで待ちます。パッケージの取得にはネットワーク接続が必要です。
4. Unity の Project ウィンドウから [Title.unity](Assets/_Project/Scenes/Title.unity) を開き、Play ▶ を押します。タイトル画面からゲームを開始し、移動とアイテム回収を試します。制限時間までプレイすると、リザルトへの遷移とスコア表示も確認できます。

| 操作 | キーボード | ゲームパッド |
| --- | --- | --- |
| 移動 | WASD または矢印キー | 左スティック |
| ジャンプ | Space | 下側のフェイスボタン |
| 攻撃 | J | 右側のフェイスボタン |
| 空中ダッシュ | K | 上側または左側のフェイスボタン |

ゲーム中の操作は [PlayerInput.cs](Assets/_Project/Features/Player/Scripts/Input/PlayerInput.cs) で定義されています。パッケージの取得に失敗したときは、Console のエラーを確認し、`git --version` が通ることと、GitHub および OpenUPM に接続できることを確認してください。パッケージの定義は [manifest.json](Packages/manifest.json) にあります。

## ディレクトリ構造

リポジトリで管理している主なフォルダとファイルです。

```text
.
├── README.md
├── Assets/
│   ├── _Project/
│   │   ├── Art/              # 共通のビジュアル素材
│   │   │   ├── Fonts/
│   │   │   ├── Sprites/
│   │   │   └── Videos/
│   │   ├── Audio/            # サウンド素材
│   │   │   ├── BGM/
│   │   │   └── SE/
│   │   ├── Core/             # 共通処理
│   │   │   ├── Events/       # ステージイベントと進行管理
│   │   │   ├── Rendering/    # スプライトのアウトラインなど
│   │   │   └── Utilities/    # ゲーム進行、カメラ、ヒットストップなど
│   │   ├── Features/         # 機能ごとのコード・Prefab・設定
│   │   │   ├── Enemy/
│   │   │   ├── Items/
│   │   │   ├── Player/
│   │   │   ├── Respawn/
│   │   │   ├── Stage/
│   │   │   ├── Treasure/
│   │   │   │   └── Tests/
│   │   │   │       └── Editor/
│   │   │   └── UI/
│   │   ├── Sandbox/          # 実験・検証用のコードと素材
│   │   │   ├── BonusIdea/
│   │   │   └── TreasureSystem/
│   │   ├── Scenes/
│   │   │   ├── Title.unity
│   │   │   ├── Game.unity
│   │   │   ├── Result.unity
│   │   │   ├── Game 1.unity
│   │   │   └── ButanoriwoWorld.unity
│   │   ├── Settings/         # URP 2D 設定とシーンテンプレート
│   │   │   └── Scenes/
│   │   └── Systems/
│   │       ├── Audio/        # シーンごとの BGM 再生
│   │       └── Score/        # スコアの加算と表示
│   ├── Editor/              # Unity Editor 用のメニューや補助ツール
│   ├── Resources/           # DOTween 設定
│   ├── TextMesh Pro/         # TextMesh Pro のリソース
│   ├── ThirdParty/
│   │   ├── DOTween/
│   │   └── MeshFlow/
│   ├── DefaultVolumeProfile.asset
│   ├── InputSystem_Actions.inputactions
│   └── UniversalRenderPipelineGlobalSettings.asset
├── Packages/                # Unity パッケージの定義とロックファイル
└── ProjectSettings/         # Unity のバージョン、ビルド対象など
```

ビルドに含まれるシーンは `Title → Game → Result` の順に登録されています。登録状況は [EditorBuildSettings.asset](ProjectSettings/EditorBuildSettings.asset) で確認できます。

## 変更したい機能の探し方

| 変更したいもの | 最初に見る場所 |
| --- | --- |
| プレイヤーの移動、攻撃、操作 | [Features/Player](Assets/_Project/Features/Player) |
| 敵の挙動、スコアアイテム | [Features/Enemy](Assets/_Project/Features/Enemy)、[Features/Items](Assets/_Project/Features/Items) |
| ステージの地形・風、時間に応じたイベント | [Stage のガイド](Assets/_Project/Features/Stage/README.md)、[Events のガイド](Assets/_Project/Core/Events/README.md) |
| 敵やアイテムの再出現、鍵と宝箱 | [Respawn のガイド](Assets/_Project/Features/Respawn/README.md)、[Treasure のガイド](Assets/_Project/Features/Treasure/README.md) |
| タイトル画面、HUD、リザルト、スコア | [Features/UI](Assets/_Project/Features/UI)、[Systems/Score](Assets/_Project/Systems/Score) |
| スプライトのアウトライン | [Rendering のガイド](Assets/_Project/Core/Rendering/README.md) |

## ファイル追加と変更後の確認

- 自作コードや素材は `Assets/_Project/` に置きます。特定機能のスクリプト、Prefab、設定は既存の `Features/<機能名>/` にまとめ、共通素材は `Art/` や `Audio/`、共通処理は `Core/`、ゲーム全体の管理処理は `Systems/` に置きます。外部アセットの配置先は `Assets/ThirdParty/` です。インポート時にツールが自動生成するファイルは、そのツールの配置に従います。
- 実験用ファイルは `Sandbox/` に置き、本番コードから参照しないようにします。
- アセットの移動や改名は Unity の Project ウィンドウで行います。追加・変更したアセットは対応する `.meta` と一緒にコミットします。`Library/`、`Logs/`、`UserSettings/` などのローカル生成物は Git の管理対象外です。
- ゲームの変更後は `Title` から Play し、変更箇所の挙動と Console のエラーを確認します。宝箱や方向表示のロジックを変更した場合は、Unity の `Window > General > Test Runner` を開き、`EditMode` タブで [Treasure のテスト](Assets/_Project/Features/Treasure/Tests/Editor) を実行します。
