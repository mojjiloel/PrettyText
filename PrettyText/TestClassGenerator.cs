using System;
using PrettyText.Utils;

class TestClassGenerator
{
    static void Main(string[] args)
    {
        Console.WriteLine("测试ClassGenerator功能：\n");

        // 测试JSON到C#类生成
        string json = @"{
            ""name"": ""张三"",
            ""age"": 30,
            ""email"": ""zhangsan@example.com"",
            ""isActive"": true,
            ""scores"": [85, 92, 78],
            ""address"": {
                ""street"": ""中山路123号"",
                ""city"": ""北京""
            }
        }";

        Console.WriteLine("输入JSON:");
        Console.WriteLine(json);
        Console.WriteLine("\n生成的C#类:");
        string csharpCode = ClassGenerator.GenerateClassFromInput(json, ClassGenerator.LanguageType.CSharp, "Person");
        Console.WriteLine(csharpCode);

        Console.WriteLine("\n" + new string('-', 50) + "\n");

        Console.WriteLine("生成的Java类:");
        string javaCode = ClassGenerator.GenerateClassFromInput(json, ClassGenerator.LanguageType.Java, "Person");
        Console.WriteLine(javaCode);

        Console.WriteLine("\n" + new string('-', 50) + "\n");

        // 测试XML到C#类生成
        string xml = @"<person>
            <name>李四</name>
            <age>25</age>
            <email>lisi@example.com</email>
            <isActive>false</isActive>
        </person>";

        Console.WriteLine("输入XML:");
        Console.WriteLine(xml);
        Console.WriteLine("\n生成的C#类:");
        string xmlCsharpCode = ClassGenerator.GenerateClassFromInput(xml, ClassGenerator.LanguageType.CSharp, "Person");
        Console.WriteLine(xmlCsharpCode);

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }
}