namespace NcmDecryptor
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: 设置传入参数
            //          -s / --single: 传入参数single 单文件模式 输出至源文件目录
            //          -f / --fold:   传入参数fold 文件夹模式 输出至源文件目录/指定输出路径
            // 传入参数为空的情况
            if (args.Length == 0)
            {
                Console.WriteLine("请提供文件路径！");
                return;
            }

            // 读取文件列表
            string outputFilePath = null;

            List<string> files = new List<string>();

            if (args.Length == 1 )
            {
                outputFilePath = args[0];
            }
            else
            {
                outputFilePath = args[1];
            }

            if (File.Exists(args[0]))
            {
                files.Add(args[0]);
            }
            else if (Directory.Exists(args[0]))
            {
                files.AddRange(Directory.GetFiles(args[0]));
            }
            else
            {
                Console.WriteLine($"路径 {args[0]} 不存在.");
            }

            foreach (string file in files)
            {
                if (Path.GetExtension(file) == ".ncm")
                {
                    NcmDecryptor.ProcessFile(file, outputFilePath);
                }
                else
                {
                    Console.WriteLine($"跳过 {file}: 不是NCM文件");
                }
            }
        }

    }
}
