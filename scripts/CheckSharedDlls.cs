// 对比两个路径下的dll文件，输出相同dll文件列表
var path1 = @"../src/Command/CommandLine/publish";
var path2 = @"../src/Services/AterStudio/publish";

var files1 = Directory.GetFiles(path1, "*.dll", SearchOption.AllDirectories).Select(Path.GetFileName).ToHashSet();
var files2 = Directory.GetFiles(path2, "*.dll", SearchOption.AllDirectories).Select(Path.GetFileName).ToHashSet();

var commonFiles = files1.Intersect(files2).ToList();

Console.WriteLine("Total common DLL files: " + commonFiles.Count);
Console.WriteLine("Common DLL files:");
Console.WriteLine(string.Join(Environment.NewLine, commonFiles.ToArray()));

