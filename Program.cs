using System.IO;
using System.Text.RegularExpressions;

namespace MarkdownHelper;

/*
 * todo
 * 程序可能存在说，会修改网络链接的情况，可能需要修复这个问题
 */

internal class Program
{
    private static string 存放图片的文件夹 = "Photos";

    private static string 当前程序所在文件文件夹路径 = AppDomain.CurrentDomain.BaseDirectory;
    private static string 程序所在文件夹内存放图片的文件夹的路径 = Path.Combine(当前程序所在文件文件夹路径, 存放图片的文件夹);

    private static void Main(string[] args)
    {
        if ((!Directory.Exists(程序所在文件夹内存放图片的文件夹的路径))) Directory.CreateDirectory(程序所在文件夹内存放图片的文件夹的路径);
        string[] files = Directory.GetFiles(当前程序所在文件文件夹路径);
        List<string> md_files = new();

        if (args.Length > 0)
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

        foreach (var md in md_files)
        {
            Console.WriteLine($"FILE: {md} ");
            var 当前文件名_带后缀 = Path.GetFileName(md);
            var 当前文件名_不带后缀 = Path.GetFileNameWithoutExtension(md);
            var 当前文件图片迁移的目标文件夹 = Path.Combine(程序所在文件夹内存放图片的文件夹的路径, 当前文件名_不带后缀);

            var 当前文件的所有内容 = File.ReadAllText(md);

            if (!Directory.Exists(当前文件图片迁移的目标文件夹)) Directory.CreateDirectory(当前文件图片迁移的目标文件夹);

            获取文件中的特定链接(out List<string> 当前文件的图片链接列表, md);
            // 获取文件中所有的图片超链接，然后对每个链接进行替换

            foreach (var 单个链接 in 当前文件的图片链接列表)
            {
                复制图片并返回新的图片超链接(单个链接, 当前文件图片迁移的目标文件夹, 当前文件名_不带后缀, out string 新的图片超链接);
                当前文件的所有内容 = 当前文件的所有内容.Replace(单个链接, 新的图片超链接);
            }
            File.WriteAllText(md, 当前文件的所有内容);
        }
    }

    private static void 获取文件中的特定链接(out List<string> 图片的链接列表, string 文件名)
    {
        图片的链接列表 = new();
        string markdownContent = File.ReadAllText(文件名);
        string pattern = @"!\[.*?\]\(.*?\)"; Regex regex = new Regex(pattern);
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
            Console.Write($"    old: {图片原来的链接地址}  new: {新的图片链接地址}\n");
            File.Copy(图片原来的链接地址, Path.Combine(迁移的目标文件夹, Path.GetFileName(图片原来的链接地址)), true);
        }
    }
}