## Which file do I download?

| File | For |
|---|---|
| **`Fm27Editor.exe`** | **Windows: the editor app. Most people only need this one.** |
| **`Fm27Editor-mac-arm64.dmg`** | **Mac (Apple silicon): the editor app.** |
| `Fm27Editor.Tools.exe` | Windows command-line tools for advanced users: `players`, `set`, `verify`, and `newbuild` / `verify-build` to support a new game build after an FM27 patch. |

Windows: each exe runs on its own, no install, no .NET needed. Windows 10/11 x64, Football Manager 27 `27.1.0+2505117` (Steam and Game Pass), `27.0.1+2503222` (Steam) and `27.0.1+2494687` (Game Pass).

Mac: open the dmg and drag FM27 Editor to Applications. macOS 13 or later, Football Manager 27 `27.1.0+2505117` and `27.0.1+2503222` (Steam), and Football Manager Touch `27.1.0+2505719` and `27.0.1+2503222` (`27.0.0+2492889` experimental). The app is not notarised: the first time macOS refuses to open it; open System Settings > Privacy & Security and click Open Anyway (on macOS 13-14, right-click the app and choose Open). On first connect it asks for your password once, to install the helper that reads the game's memory.

## Tải file nào?

| File | Dùng để |
|---|---|
| **`Fm27Editor.exe`** | **Windows: app editor. Đa số người dùng chỉ cần file này.** |
| **`Fm27Editor-mac-arm64.dmg`** | **Mac (chip Apple): app editor.** |
| `Fm27Editor.Tools.exe` | Công cụ dòng lệnh trên Windows cho người dùng nâng cao: `players`, `set`, `verify`, và `newbuild` / `verify-build` để hỗ trợ bản game mới sau khi FM27 có patch. |

Windows: mỗi file exe chạy độc lập, không cần cài đặt, không cần .NET. Windows 10/11 x64, Football Manager 27 `27.1.0+2505117` (Steam và Game Pass), `27.0.1+2503222` (Steam) và `27.0.1+2494687` (Game Pass).

Mac: mở file dmg rồi kéo FM27 Editor vào Applications. macOS 13 trở lên, Football Manager 27 `27.1.0+2505117` và `27.0.1+2503222` (Steam), và Football Manager Touch `27.1.0+2505719` và `27.0.1+2503222` (`27.0.0+2492889` thử nghiệm). App chưa được Apple notarise: lần đầu macOS sẽ chặn; vào System Settings > Privacy & Security và bấm Open Anyway (macOS 13-14: nhấp chuột phải vào app và chọn Open). Lần kết nối đầu tiên app hỏi mật khẩu một lần để cài helper đọc bộ nhớ game.

## Verify / Kiểm chứng

Built by GitHub Actions from commit `{SHA}` (tag `{TAG}`). Compare hashes with `SHA256SUMS.txt`, or check the signed build attestation:

Được GitHub Actions build từ commit `{SHA}` (tag `{TAG}`). So hash với `SHA256SUMS.txt`, hoặc kiểm chứng thực build có chữ ký:

```
gh attestation verify Fm27Editor.exe --repo Jackhamerp24/Football-Manager-27-Editor
gh attestation verify Fm27Editor-mac-arm64.dmg --repo Jackhamerp24/Football-Manager-27-Editor
```

## 简体中文说明

本版本首次提供简体中文界面选项。常用导航、设置、搜索、部分球员与俱乐部字段及常用操作已翻译；尚未覆盖的内容继续显示英文。启动编辑器后，打开 Settings，将 Language 设为 **简体中文**。建议先在备份存档上验证，再进行正式编辑。
