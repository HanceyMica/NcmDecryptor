using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using TagLib;

namespace NcmDecryptor
{
    public class NcmDecryptor
    {
        private static readonly byte[] AesCoreKey = { 0x68, 0x7A, 0x48, 0x52, 0x41, 0x6D, 0x73, 0x6F, 0x35, 0x6B, 0x49, 0x6E, 0x62, 0x61, 0x78, 0x57 };
        private static readonly byte[] AesModifyKey = { 0x23, 0x31, 0x34, 0x6C, 0x6A, 0x6B, 0x5F, 0x21, 0x5C, 0x5D, 0x26, 0x30, 0x55, 0x3C, 0x27, 0x28 };

        // 构建密钥表
        static byte[] BuildKeyBox(byte[] key)
        {
            byte[] box = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                box[i] = (byte)i;
            }
            byte keyLen = (byte)key.Length;
            byte c = 0;
            byte lastByte = 0;
            byte keyOffset = 0;
            for (int i = 0; i < 256; i++)
            {
                c = (byte)((box[i] + lastByte + key[keyOffset]) & 0xff);
                keyOffset++;
                if (keyOffset >= keyLen)
                {
                    keyOffset = 0;
                }
                (box[i], box[c]) = (box[c], box[i]);
                lastByte = c;
            }
            return box;
        }

        // 修复数据块大小
        static byte[] FixBlockSize(byte[] src)
        {
            int blockSize = 16; // 128-bit // 128位 
            return src.Take(src.Length / blockSize * blockSize).ToArray();
        }

        // 检查数据是否包含PNG头部
        static bool ContainPNGHeader(byte[] data)
        {
            if (data.Length < 8)
            {
                return false;
            }
            return data.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        }

        // PKCS7去填充
        static byte[] PKCS7UnPadding(byte[] src)
        {
            int length = src.Length;
            int unpadding = (int)src[length - 1];
            return src.Take(length - unpadding).ToArray();
        }

        // 使用AES-128-ECB解密
        static byte[] DecryptAes128Ecb(byte[] key, byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;
                aes.Key = key;

                ICryptoTransform decryptor = aes.CreateDecryptor();

                byte[] decrypted = new byte[data.Length];
                int blockSize = aes.BlockSize / 8;
                int bytesRead = 0;

                using (MemoryStream memoryStream = new MemoryStream(data))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        bytesRead = cryptoStream.Read(decrypted, 0, decrypted.Length);
                    }
                }

                Array.Resize(ref decrypted, bytesRead);
                return PKCS7UnPadding(decrypted);
            }
        }

        // 读取32位无符号整数
        static uint ReadUint32(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        // 处理文件
        public static void ProcessFile(string inputFilePath, string outputFilePath)
        {
            using (FileStream fileStream = System.IO.File.OpenRead(inputFilePath))
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                uint uLen = ReadUint32(reader);
                if (uLen != 0x4e455443)
                {
                    Console.WriteLine("不是网易云音乐加密文件！");
                    return;
                }

                uLen = ReadUint32(reader);
                if (uLen != 0x4d414446)
                {
                    Console.WriteLine("不是网易云音乐加密文件！");
                    return;
                }

                reader.BaseStream.Seek(2, SeekOrigin.Current);
                uLen = ReadUint32(reader);

                byte[] keyData = reader.ReadBytes((int)uLen);

                for (int i = 0; i < keyData.Length; i++)
                {
                    keyData[i] ^= 0x64;
                }

                byte[] deKeyData = DecryptAes128Ecb(AesCoreKey, FixBlockSize(keyData));

                // 17 = len("neteasecloudmusic")
                deKeyData = deKeyData.Skip(17).ToArray();

                uLen = ReadUint32(reader);
                byte[] modifyData = reader.ReadBytes((int)uLen);

                for (int i = 0; i < modifyData.Length; i++)
                {
                    modifyData[i] ^= 0x63;
                }

                byte[] deModifyData = Convert.FromBase64String(Encoding.ASCII.GetString(modifyData, 22, modifyData.Length - 22));

                byte[] deData = DecryptAes128Ecb(AesModifyKey, FixBlockSize(deModifyData));

                // 6 = len("music:")
                deData = deData.Skip(6).ToArray();

                string jsonString = Encoding.UTF8.GetString(deData);
                MetaInfo meta = JsonConvert.DeserializeObject<MetaInfo>(jsonString);

                reader.BaseStream.Seek(4, SeekOrigin.Current);
                reader.BaseStream.Seek(5, SeekOrigin.Current);

                uint imgLen = ReadUint32(reader);

                byte[] imgData = imgLen > 0 ? reader.ReadBytes((int)imgLen) : null;

                byte[] box = BuildKeyBox(deKeyData);
                int n = 0x8000;

                // output file
                string outputName = Path.ChangeExtension(outputFilePath, meta.Format);

                using (FileStream outputStream = System.IO.File.Create(outputName))
                {
                    byte[] tb = new byte[n];
                    int bytesRead;
                    while ((bytesRead = reader.Read(tb, 0, n)) > 0)
                    {
                        for (int i = 0; i < bytesRead; i++)
                        {
                            byte j = (byte)((i + 1) & 0xff);
                            tb[i] ^= box[(box[j] + box[(box[j] + j) & 0xff]) & 0xff];
                        }
                        outputStream.Write(tb, 0, bytesRead);
                    }
                }

                Console.WriteLine(outputName);
                switch (meta.Format)
                {
                    case "mp3":
                        AddMP3Tag(outputName, imgData, meta);
                        break;
                    case "flac":
                        AddFLACTag(outputName, imgData, meta);
                        break;
                }
            }
        }

        // 获取URL内容
        static byte[] FetchUrl(string url)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    return client.DownloadData(url);
                }
                catch (WebException ex)
                {
                    Console.WriteLine($"下载专辑图片失败: 远程服务器返回错误代码 {((HttpWebResponse)ex.Response).StatusCode}");
                    return null;
                }
            }
        }

        // 为FLAC文件添加标签
        static void AddFLACTag(string filePath, byte[] imgData, MetaInfo meta)
        {
            TagLib.Flac.File flacFile = TagLib.File.Create(filePath) as TagLib.Flac.File;

            if (imgData == null && !string.IsNullOrEmpty(meta.AlbumPic))
            {
                imgData = FetchUrl(meta.AlbumPic);
            }

            if (imgData != null)
            {
                string picMIME = ContainPNGHeader(imgData) ? "image/png" : "image/jpeg";
                Picture picture = new Picture
                {
                    Type = PictureType.FrontCover,
                    MimeType = picMIME,
                    Description = "Front cover",
                    Data = imgData
                };
                flacFile.Tag.Pictures = new IPicture[] { picture };
            }
            else if (!string.IsNullOrEmpty(meta.AlbumPic))
            {
                Picture picture = new Picture
                {
                    Type = PictureType.FrontCover,
                    MimeType = "-->",
                    Description = "Front cover",
                    Data = Encoding.UTF8.GetBytes(meta.AlbumPic)
                };
                flacFile.Tag.Pictures = new IPicture[] { picture };
            }

            if (string.IsNullOrEmpty(flacFile.Tag.Title) && !string.IsNullOrEmpty(meta.MusicName))
            {
                Console.WriteLine("添加音乐名称");
                flacFile.Tag.Title = meta.MusicName;
            }

            if (string.IsNullOrEmpty(flacFile.Tag.Album) && !string.IsNullOrEmpty(meta.Album))
            {
                Console.WriteLine("添加专辑名称");
                flacFile.Tag.Album = meta.Album;
            }

            if (flacFile.Tag.Performers.Length == 0 && meta.Artist.Count > 0)
            {
                List<string> artists = meta.Artist.Select(a => a[0].ToString()).ToList();
                Console.WriteLine("添加艺术家");
                foreach (string artist in artists)
                {
                    flacFile.Tag.Performers = flacFile.Tag.Performers.Concat(new[] { artist }).ToArray();
                }
            }

            flacFile.Save();
        }

        // 为MP3文件添加标签
        static void AddMP3Tag(string filePath, byte[] imgData, MetaInfo meta)
        {
            TagLib.File tagFile = TagLib.File.Create(filePath);

            if (imgData == null && !string.IsNullOrEmpty(meta.AlbumPic))
            {
                imgData = FetchUrl(meta.AlbumPic);
            }

            if (imgData != null)
            {
                string picMIME = ContainPNGHeader(imgData) ? "image/png" : "image/jpeg";
                TagLib.Id3v2.AttachedPictureFrame picFrame = new TagLib.Id3v2.AttachedPictureFrame
                {
                    TextEncoding = TagLib.StringType.UTF16,
                    MimeType = picMIME,
                    Type = TagLib.PictureType.FrontCover,
                    Description = "Front cover",
                    Data = imgData
                };
                tagFile.Tag.Pictures = new TagLib.IPicture[] { picFrame };
            }
            else if (!string.IsNullOrEmpty(meta.AlbumPic))
            {
                TagLib.Id3v2.AttachedPictureFrame picFrame = new TagLib.Id3v2.AttachedPictureFrame
                {
                    TextEncoding = TagLib.StringType.UTF16,
                    MimeType = "-->",
                    Type = TagLib.PictureType.FrontCover,
                    Description = "Front cover",
                    Data = Encoding.UTF8.GetBytes(meta.AlbumPic)
                };
                tagFile.Tag.Pictures = new TagLib.IPicture[] { picFrame };
            }

            if (string.IsNullOrEmpty(tagFile.Tag.Title) && !string.IsNullOrEmpty(meta.MusicName))
            {
                Console.WriteLine("添加音乐名称");
                tagFile.Tag.Title = meta.MusicName;
            }

            if (string.IsNullOrEmpty(tagFile.Tag.Album) && !string.IsNullOrEmpty(meta.Album))
            {
                Console.WriteLine("添加专辑名称");
                tagFile.Tag.Album = meta.Album;
            }

            if (tagFile.Tag.Performers.Length == 0 && meta.Artist.Count > 0)
            {
                List<string> artists = meta.Artist.Select(a => a[0].ToString()).ToList();
                Console.WriteLine("添加艺术家");
                foreach (string artist in artists)
                {
                    tagFile.Tag.Performers = tagFile.Tag.Performers.Concat(new[] { artist }).ToArray();
                }
            }

            tagFile.Save();
        }
    }
}
