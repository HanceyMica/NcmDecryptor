using Newtonsoft.Json;

namespace NcmDecryptor
{
    internal class MetaInfo
    {
        [JsonProperty("musicId")]
        public int MusicID { get; set; } // 音乐ID

        [JsonProperty("musicName")]
        public string MusicName { get; set; } // 音乐名称

        [JsonProperty("artist")]
        public List<List<object>> Artist { get; set; } // 艺术家列表

        [JsonProperty("albumId")]
        public int AlbumID { get; set; } // 专辑ID

        [JsonProperty("album")]
        public string Album { get; set; } // 专辑名称

        [JsonProperty("albumPicDocId")]
        public object AlbumPicDocID { get; set; } // 专辑封面文档ID

        [JsonProperty("albumPic")]
        public string AlbumPic { get; set; } // 专辑封面URL

        [JsonProperty("bitrate")]
        public int BitRate { get; set; } // 比特率

        [JsonProperty("mp3DocId")]
        public string Mp3DocID { get; set; } // MP3文档ID

        [JsonProperty("duration")]
        public int Duration { get; set; } // 音乐时长

        [JsonProperty("mvId")]
        public int MvID { get; set; } // MV ID

        [JsonProperty("alias")]
        public List<string> Alias { get; set; } // 音乐别名列表

        [JsonProperty("transNames")]
        public List<object> TransNames { get; set; } // 音乐翻译名称列表

        [JsonProperty("format")]
        public string Format { get; set; } // 音乐格式
    }

}
