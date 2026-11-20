# CLAUDE.md

ONI mod。ビルド: `dotnet build MoveToHotkeyDebrisToo.sln`（Debug は `Local.Build.props` の `ModInstallPath` へコピーする。`MoveToHotkeyDebrisToo/` 配下に置く。確認だけなら `-p:ModInstallPath=`）。csproj は `MoveToHotkeyDebrisToo/` の下にあり、`harness.json` は repo 直下。

## 何をする Mod か
「移動先（複製人間）」「移動先（ガラクタ）」の 2 つのホットキーを、キー設定画面（PLib 経由）に追加する。既定は両方ホイールクリック（Mouse 2）。U60 でバニラにも Move To のボタンができたが Q 固定でキー設定に出ないため、`UserMenu.AddButton` のプレフィックスでボタンのショートカットとツールチップをこの Mod のアクションに差し替える（バニラの Q も効く）。U60 と U59 の両方で動く（`minimumSupportedBuild` 744825。U59 のバニラのボタンにはショートカットもツールチップの `{Hotkey}` もないので、ボタンに割り当てるだけでツールチップにキーは出ない）。コード（テスト以外）は U59 でも変更なし。`u59` ブランチはテストを U59 で通すための参照用（`Action.MoveRelocateTo` を使わない）。

## 構成
- `ModActions.cs`: PLib のアクション登録と、翻訳後のタイトル更新（`RefreshTitles`。翻訳で STRINGS のフィールドが別オブジェクトに置き換わるため。`LocString` を `object` で保持するのは、ゲームが名前空間内の null の `LocString` フィールドでつまずくのを避けるため）。
- `Patches/UserMenuPatches.cs`: バニラのボタンの付け替え。ツールチップはバニラが `Q` を焼き込むので `{Hotkey}` のテンプレートに戻す。
- `Patches/InputBindingsScreenPatches.cs`: キー設定画面で Mouse 2 と Esc（解除）を受け付ける。重複判定はこの Mod の 2 アクション同士と MouseMiddle を除外する（バニラの規則は保つ）。
- `Patches/MainMenuPatches.cs`: タイトル画面の重複キー検査で、この Mod のバインドが片方外されないようにする。
- `STRINGS` のフィールドは readonly にしない（翻訳適用がリフレクションで置き換えるため）。

## 実機検証
`HarnessTests.cs`（`#if DEBUG`、タグ `movetohotkeydebristoo`）。要 `Local.Build.props` の `HarnessDllPath`。
実行: `python C:/Users/dha_1/ONI-Modding/GravitasTestHarness/Gravitas.TestProbe/probe.py run --project . --summary --tests '{"tags":["movetohotkeydebristoo"]}'`
ゲームを起動し本番の `mods.json` を書き換えるので、実行前にユーザーに確認する。
テストが見ていないもの: キー設定画面でのホイールクリックの実際の割り当て（`InputBindingsScreen.Update` のパッチ）、画面の見た目。
