# Move To Hotkey Debris Too

Oxygen Not Included 用 Mod。ホットキー設定画面に 2 つの「Move To」アクションを追加します。

- Move To (Duplicant)
- Move To (Debris)

## 機能

- 上記 2 つのホットキーを、ゲームのホットキー設定画面から割り当て可能（PLib 使用）
- ホットキー設定画面でマウスのホイールクリック（Mouse 2）を割り当て可能
- キー割り当て待ちの状態で Esc を押すと、割り当てを None に設定
- 本 Mod の 2 つのホットキー同士は、同じキーを重複して割り当て可能
  （重複検出で他方が解除されません。タイトル画面の重複チェックでも解除されません）

## 注意

- 初期割り当ては両アクションともホイールクリック（Mouse 2）です。
- ホイールクリックは、ゲーム標準の `MouseMiddle` と重複して見える場合がありますが、本 Mod のアクションに限り解除されずに割り当てられます。

## 依存

- [PLib](https://github.com/peterhaneve/ONIMods)（ILRepack でビルドに統合）
- Harmony

## ビルド

`Local.Build.props` にゲームの `Managed` フォルダを指定し、Rider（または .NET SDK 8.0）でビルドします。
ビルド後、DLL は `ModInstallPath` の `mods\local` にコピーされます。ゲーム起動中は DLL がロックされるため、ゲームを終了してからビルドしてください。
