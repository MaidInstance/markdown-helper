using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MarkdownHelper;

internal class Program
{
    private static string 存放图片的文件夹 = "Photos";
    private static readonly string ConfigFileLocation = "./mh_config.json";

    private static readonly string 当前程序所在文件文件夹路径 = AppDomain.CurrentDomain.BaseDirectory;
    private static string 程序所在文件夹内存放图片的文件夹的路径 = Path.Combine(当前程序所在文件文件夹路径, 存放图片的文件夹);

    private static void Main(string[] args)
    {
        bool hasCommandLineArgs = args.Length > 0;

        string[] files = Directory.GetFiles(当前程序所在文件文件夹路径);
        List<string> md_files = [];

        if (hasCommandLineArgs)
        {
            foreach (var 文件地址 in args)
            {
                if (File.Exists(文件地址) && 文件地址.EndsWith(".md")) md_files.Add(文件地址);
            }
            Console.WriteLine("通过参数传递执行");
        }
        else
        {
            foreach (string file in files) if (file.EndsWith(".md")) md_files.Add(file);
        }

        // 配置文件是否存在，如果不存在使用默认配置执行
        JObject? j = null;
        if (File.Exists(ConfigFileLocation))
        {
            j = JObject.Parse(File.ReadAllText(ConfigFileLocation));
        }
        if (j is null)
        {
            Console.WriteLine("没有找到JSON文件，默认执行");
            JustDoIt(md_files);
            return;
        }

        LoadConfig(j, out bool isDefault, out bool folderIsChanged, out string folderName, out string folderName_);

        // 如果配置文件存在错误不能正确读取值，就默认执行
        if (isDefault)
        {
            Console.WriteLine("已经读取到了文件，读取错误，默认执行");
            JustDoIt(md_files);
        }
        // 使用配置文件的文件夹名字复制给程序，后面的执行就不会是默认执行
        UseNewSettings(folderName);
        JustDoIt(md_files);
        if (folderIsChanged)
        {
            Console.WriteLine("图片已经改变，更改图片位置中");
            var oldFolderPath = Path.Combine(当前程序所在文件文件夹路径, folderName_);
            if (Path.Exists(oldFolderPath)) Directory.Delete(oldFolderPath, true);
            j["FolderName_bak"] = folderName;

            File.WriteAllText(ConfigFileLocation, Convert.ToString(j));
        }
    }

    private static void JustDoIt(List<string> md_files)
    {
        if ((!Directory.Exists(程序所在文件夹内存放图片的文件夹的路径))) Directory.CreateDirectory(程序所在文件夹内存放图片的文件夹的路径);
        foreach (var md in md_files)
        {
            Console.WriteLine($"FILE: {md} ");
            // var 当前文件名_带后缀 = Path.GetFileName(md);
            var 当前文件名_不带后缀 = Path.GetFileNameWithoutExtension(md);
            var 当前文件图片迁移的目标文件夹 = Path.Combine(程序所在文件夹内存放图片的文件夹的路径, 当前文件名_不带后缀);

            var 当前文件的所有内容 = File.ReadAllText(md);

            if (!Directory.Exists(当前文件图片迁移的目标文件夹)) Directory.CreateDirectory(当前文件图片迁移的目标文件夹);

            获取文件中的特定链接(out List<string> 当前文件的图片链接列表, md);

            // 获取文件中所有的图片超链接，然后对每个链接进行替换
            foreach (var 单个链接 in 当前文件的图片链接列表)
            {
                if (IsWebUrl(单个链接)) continue; // 或许有用吧，我不知道
                复制图片并返回新的图片超链接(单个链接, 当前文件图片迁移的目标文件夹, 当前文件名_不带后缀, out string 新的图片超链接);
                当前文件的所有内容 = 当前文件的所有内容.Replace(单个链接, 新的图片超链接);
            }
            File.WriteAllText(md, 当前文件的所有内容);
        }
    }

    private static bool IsFolderNameValid(string folderName)
    {
        if (folderName.Length > 128)
        {
            return false;
        }

        string illegalPattern = @"[\\/:\*\?""<>|]";
        Regex regex = new(illegalPattern);
        if (regex.IsMatch(folderName))
        {
            return false;
        }

        if (folderName.StartsWith(' ') || folderName.StartsWith('.') ||
            folderName.EndsWith(' ') || folderName.EndsWith('.'))
        {
            return false;
        }

        if (string.IsNullOrEmpty(folderName))
        {
            return false;
        }

        return true;
    }

    private static void UseNewSettings(string folderName)
    {
        存放图片的文件夹 = folderName;
        程序所在文件夹内存放图片的文件夹的路径 = Path.Combine(当前程序所在文件文件夹路径, 存放图片的文件夹);
    }

    private static void LoadConfig(JObject config, out bool defaultMode, out bool folderIsChanged, out string newFolderName, out string oldFolderName)
    {
        defaultMode = true;
        folderIsChanged = false;
        newFolderName = 存放图片的文件夹;
        oldFolderName = 存放图片的文件夹;
        try
        {
            if (config["FolderName"] is not null && config["FolderName_bak"] is not null)
            {
                var folderName_ = config["FolderName"].ToString();
                var folderName_bak = config["FolderName_bak"].ToString();
                defaultMode = false;
                if (IsFolderNameValid(folderName_)) newFolderName = folderName_;
                if (IsFolderNameValid(folderName_bak)) oldFolderName = folderName_bak;
                folderIsChanged = !folderName_.Equals(folderName_bak);
            }
            else defaultMode = true;
        }
        catch
        {
            Debug.WriteLine("配置文件错误");
        }
    }

    private static bool IsWebUrl(string url)
    {
        return url.StartsWith("https") || url.StartsWith("http");
    }

    private static void 获取文件中的特定链接(out List<string> 图片的链接列表, string 文件名)
    {
        图片的链接列表 = [];
        string markdownContent = File.ReadAllText(文件名);
        string pattern = @"!\[.*?\]\(.*?\)";
        Regex regex = new(pattern);
        MatchCollection matches = regex.Matches(markdownContent);

        foreach (Match match in matches)
        {
            图片的链接列表.Add(match.Value);
        }
    }

    private static void 复制图片并返回新的图片超链接(string 图片的超链接, string 迁移的目标文件夹, string 当前文件名_不带后缀, out string 新的图片超链接)
    {
        string pattern = @"\[([^\]]+)\]\(([^)]+)\)";
        Match match = Regex.Match(图片的超链接, pattern);

        string 图片名 = "", 图片原来的链接地址 = "";

        if (match.Success)
        {
            图片名 = match.Groups[1].Value;
            图片原来的链接地址 = match.Groups[2].Value;
        }

        // Console.WriteLine("origin " + 图片原来的链接地址);

        var 新的图片链接地址 = Path.Combine($".\\{存放图片的文件夹}\\{当前文件名_不带后缀}", Path.GetFileName(图片原来的链接地址));
        新的图片链接地址 = 新的图片链接地址.Replace('\\', '/');
        新的图片超链接 = $"![{图片名}]({新的图片链接地址})";

        if (图片原来的链接地址.Equals(新的图片链接地址)) return;
        if (File.Exists(图片原来的链接地址))
        {
            Console.Write($"    old: {图片原来的链接地址}\n    new: {新的图片链接地址}\n");
            File.Copy(图片原来的链接地址, Path.Combine(迁移的目标文件夹, Path.GetFileName(图片原来的链接地址)), true);
        }
    }
}