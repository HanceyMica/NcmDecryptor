# NcmDecryptor

将网易云音乐 NCM 格式加密的音乐转换为其他可被播放器接受的格式（MP3/FLAC）。

## ✨ 功能特点

- 🔓 **完整解密**：解密网易云音乐加密的 NCM 文件
- 🎵 **保留元数据**：自动保留并写入歌曲名称、艺术家、专辑、封面等信息
- 🖼️ **专辑封面**：支持嵌入专辑封面图片（支持 JPEG 和 PNG 格式）
- 📁 **批量处理**：支持文件夹批量转换
- 🎯 **智能识别**：自动识别文件/文件夹类型

## 📋 系统要求

- .NET 6.0 或更高版本
- Windows/Linux/macOS（支持 .NET 的平台）

## 🚀 安装说明

### 从源码编译

```bash
# 克隆项目
git clone https://github.com/HanceyMica/NcmDecryptor.git

# 进入项目目录
cd NcmDecryptor

# 编译项目
dotnet build NcmDecryptor/NcmDecryptor.csproj

# 运行程序
dotnet run --project NcmDecryptor/NcmDecryptor.csproj [参数]
```

### 发布为独立程序

```bash
dotnet publish NcmDecryptor/NcmDecryptor.csproj -c Release -r <运行时标识> --self-contained
```

支持的运行时标识：
- `win-x64` - Windows x64
- `linux-x64` - Linux x64
- `osx-x64` - macOS x64
- `osx-arm64` - macOS ARM64

## 📖 使用方法

### 基本用法

```bash
# 处理单个文件（自动检测）
NcmDecryptor song.ncm

# 处理文件夹（自动检测）
NcmDecryptor ./music/

# 显示帮助信息
NcmDecryptor -h
```

### 命令行参数

| 参数 | 说明 |
|------|------|
| `-s, --single <文件路径>` | 单文件模式，处理单个 NCM 文件 |
| `-f, --fold <文件夹路径>` | 文件夹模式，处理文件夹中的所有 NCM 文件 |
| `-h, --help` | 显示帮助信息 |

### 使用示例

```bash
# 单文件模式
NcmDecryptor -s song.ncm
NcmDecryptor --single song.ncm

# 单文件模式，指定输出目录
NcmDecryptor -s song.ncm output/
NcmDecryptor --single song.ncm ./decrypted/

# 文件夹模式
NcmDecryptor -f ./music/
NcmDecryptor --fold ./music/

# 文件夹模式，指定输出目录
NcmDecryptor -f ./music/ output/
NcmDecryptor --fold ./music/ ./decrypted/

# 自动检测模式
NcmDecryptor song.ncm                    # 自动识别为单文件
NcmDecryptor ./music/                    # 自动识别为文件夹
```

## 🔧 工作原理

NCM 文件的加密和解密流程：

1. **文件格式验证**：检查 NCM 文件头（`NETC` + `MADA`）
2. **密钥解密**：
   - 使用 XOR 运算（密钥：`0x64`）初步解密
   - 使用 AES-128-ECB 算法解密获取音乐解密密钥
3. **元数据解密**：
   - 使用 XOR 运算（密钥：`0x63`）和 Base64 解码
   - 使用 AES-128-ECB 解密获取元数据信息
   - 解析 JSON 格式的歌曲信息
4. **音频数据解密**：使用 KeyBox 算法（类似 RC4 流加密）解密音频数据
5. **标签写入**：使用 TagLibSharp 库将元数据（标题、艺术家、专辑、封面）写入输出文件

## 📦 支持的输出格式

- **MP3**：完整的 ID3v2 标签支持
- **FLAC**：完整的 Vorbis Comments 标签支持

输出文件自动命名为 `{歌曲名称}.{格式}`，例如：`晴天.mp3`

## ⚠️ 注意事项

- 解密后的文件仅供个人学习使用
- 请尊重版权，不要传播解密后的文件
- 程序会自动跳过非 NCM 格式的文件
- 如果文件已包含完整的元数据标签，不会重复写入

## 🐛 常见问题

### Q: 解密失败怎么办？

A: 请确保：
- NCM 文件未损坏
- .NET 运行时版本 >= 6.0
- 文件权限正常（可读可写）

### Q: 为什么专辑封面没有显示？

A: 可能的原因：
- NCM 文件中未包含封面数据
- 播放器不支持嵌入的封面图片
- 封面数据损坏

### Q: 支持哪些音频格式？

A: 目前支持输出为 MP3 和 FLAC 格式。输入仅支持网易云音乐的 NCM 格式。

## 📄 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

## 🙏 致谢

- [Newtonsoft.Json](https://www.newtonsoft.com/json) - JSON 解析库
- [TagLibSharp](https://github.com/mono/taglib-sharp) - 音频标签读写库

## 👤 作者

**HanceyMica**

- GitHub: [HanceyMica/NcmDecryptor](https://github.com/HanceyMica/NcmDecryptor)

## 📝 更新日志

### v1.0.0
- ✅ 实现 NCM 文件解密功能
- ✅ 支持 MP3 和 FLAC 格式输出
- ✅ 自动保留元数据和专辑封面
- ✅ 支持单文件和文件夹批量处理
- ✅ 支持自定义输出目录
- ✅ 提供命令行参数选项

---

*如果你觉得这个项目有用，请给它一个 ⭐ Star！*
