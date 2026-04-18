namespace NcmDecryptor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            string mode = null;
            string inputPath = null;
            string outputPath = null;
            
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                
                if (arg == "-s" || arg == "--single")
                {
                    mode = "single";
                }
                else if (arg == "-f" || arg == "--fold")
                {
                    mode = "fold";
                }
                else if (arg == "-h" || arg == "--help")
                {
                    PrintUsage();
                    return;
                }
                else if (inputPath == null)
                {
                    inputPath = arg;
                }
                else if (outputPath == null)
                {
                    outputPath = arg;
                }
            }

            if (string.IsNullOrEmpty(inputPath))
            {
                Console.WriteLine("错误: 请提供输入路径！");
                PrintUsage();
                return;
            }

            List<string> files = new List<string>();

            if (mode == "single")
            {
                if (File.Exists(inputPath))
                {
                    files.Add(inputPath);
                    string directory = Path.GetDirectoryName(inputPath);
                    outputPath = !string.IsNullOrEmpty(outputPath) ? outputPath : directory;
                }
                else
                {
                    Console.WriteLine($"错误: 单文件模式下，路径 {inputPath} 不存在或不是文件。");
                    return;
                }
            }
            else if (mode == "fold")
            {
                if (Directory.Exists(inputPath))
                {
                    files.AddRange(Directory.GetFiles(inputPath));
                    outputPath = !string.IsNullOrEmpty(outputPath) ? outputPath : inputPath;
                }
                else
                {
                    Console.WriteLine($"错误: 文件夹模式下，路径 {inputPath} 不存在或不是文件夹。");
                    return;
                }
            }
            else if (File.Exists(inputPath))
            {
                files.Add(inputPath);
                string directory = Path.GetDirectoryName(inputPath);
                outputPath = !string.IsNullOrEmpty(outputPath) ? outputPath : directory;
            }
            else if (Directory.Exists(inputPath))
            {
                files.AddRange(Directory.GetFiles(inputPath));
                outputPath = !string.IsNullOrEmpty(outputPath) ? outputPath : inputPath;
            }
            else
            {
                Console.WriteLine($"错误: 路径 {inputPath} 不存在。");
                return;
            }

            foreach (string file in files)
            {
                if (Path.GetExtension(file) == ".ncm")
                {
                    NcmDecryptor.ProcessFile(file, outputPath);
                }
                else
                {
                    Console.WriteLine($"跳过 {file}: 不是NCM文件");
                }
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("NcmDecryptor - 网易云音乐NCM格式解密工具");
            Console.WriteLine();
            Console.WriteLine("用法:");
            Console.WriteLine("  NcmDecryptor -s <文件路径> [输出目录]");
            Console.WriteLine("  NcmDecryptor -f <文件夹路径> [输出目录]");
            Console.WriteLine("  NcmDecryptor <文件或文件夹路径>");
            Console.WriteLine();
            Console.WriteLine("参数:");
            Console.WriteLine("  -s, --single    单文件模式，处理单个NCM文件");
            Console.WriteLine("  -f, --fold      文件夹模式，处理文件夹中的所有NCM文件");
            Console.WriteLine("  -h, --help      显示帮助信息");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  NcmDecryptor song.ncm");
            Console.WriteLine("  NcmDecryptor -s song.ncm");
            Console.WriteLine("  NcmDecryptor -s song.ncm output/");
            Console.WriteLine("  NcmDecryptor -f ./music/");
            Console.WriteLine("  NcmDecryptor -f ./music/ output/");
        }

    }
}
