using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml;

namespace PrettyText.Utils
{
    public class ClassGenerator
    {
        public enum LanguageType
        {
            CSharp,
            Java
        }

        /// <summary>
        /// 根据JSON生成类定义
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <param name="language">目标语言</param>
        /// <param name="className">类名</param>
        /// <returns>生成的类代码</returns>
        public static string GenerateClassFromJson(string json, LanguageType language, string className = "GeneratedClass")
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                var obj = serializer.DeserializeObject(json);
                
                if (obj is Dictionary<string, object> dict)
                {
                    return GenerateClassFromDictionary(dict, language, className);
                }
                else if (obj is object[] array)
                {
                    // 如果是数组，取第一个元素作为模板
                    if (array.Length > 0 && array[0] is Dictionary<string, object> firstItem)
                    {
                        return GenerateClassFromDictionary(firstItem, language, className);
                    }
                    else
                    {
                        return "// 无法从空数组或非对象数组生成类";
                    }
                }
                else
                {
                    return "// 无法解析JSON为对象或对象数组";
                }
            }
            catch (Exception ex)
            {
                return $"// JSON解析错误: {ex.Message}";
            }
        }

        /// <summary>
        /// 根据XML生成类定义
        /// </summary>
        /// <param name="xml">XML字符串</param>
        /// <param name="language">目标语言</param>
        /// <param name="className">类名</param>
        /// <returns>生成的类代码</returns>
        public static string GenerateClassFromXml(string xml, LanguageType language, string className = "GeneratedClass")
        {
            try
            {
                var doc = new XmlDocument();
                doc.XmlResolver = null;
                doc.LoadXml(xml);

                var rootElement = doc.DocumentElement;
                var properties = ExtractPropertiesFromXml(rootElement);
                
                // 查找嵌套类定义
                var nestedClasses = new Dictionary<string, XmlNode>();
                FindNestedClasses(rootElement, nestedClasses);
                
                var sb = new StringBuilder();
                
                // 生成主类
                sb.Append(GenerateClassFromProperties(properties, language, className));
                
                // 生成嵌套类
                foreach (var nestedClass in nestedClasses)
                {
                    var nestedProperties = ExtractPropertiesFromXml(nestedClass.Value);
                    sb.AppendLine();
                    sb.Append(GenerateClassFromProperties(nestedProperties, language, nestedClass.Key));
                }
                
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"// XML解析错误: {ex.Message}";
            }
        }
        
        /// <summary>
        /// 递归查找嵌套类（元素）
        /// </summary>
        /// <param name="node">XML节点</param>
        /// <param name="nestedClasses">嵌套类集合</param>
        private static void FindNestedClasses(XmlNode node, Dictionary<string, XmlNode> nestedClasses)
        {
            if (node.ChildNodes != null)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        // 检查子元素是否有自己的子元素（即嵌套对象）
                        var childChildElements = child.ChildNodes.Cast<XmlNode>()
                            .Where(n => n.NodeType == XmlNodeType.Element)
                            .ToList();
                        
                        if (childChildElements.Any() && !nestedClasses.ContainsKey(child.Name))
                        {
                            // 这是一个嵌套对象
                            nestedClasses[child.Name] = child;
                        }
                        
                        // 递归处理子元素
                        FindNestedClasses(child, nestedClasses);
                    }
                }
            }
        }

        private static Dictionary<string, string> ExtractPropertiesFromXml(XmlNode node)
        {
            var properties = new Dictionary<string, string>();

            // 处理属性
            if (node.Attributes != null)
            {
                foreach (XmlAttribute attr in node.Attributes)
                {
                    string propertyName = ToPascalCase(attr.Name);
                    properties[propertyName] = GetDataTypeFromValue(attr.Value);
                }
            }

            // 处理子元素
            if (node.ChildNodes != null)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        string propertyName = ToPascalCase(child.Name);
                        
                        // 检查是否是列表（多个同名元素）
                        var siblingElements = node.ChildNodes.Cast<XmlNode>()
                            .Where(n => n.NodeType == XmlNodeType.Element && n.Name == child.Name)
                            .ToList();
                        
                        if (siblingElements.Count > 1)
                        {
                            // 这是一个列表
                            properties[propertyName + "s"] = "List<" + GetDataTypeFromXml(child) + ">";
                        }
                        else
                        {
                            // 检查子元素是否有自己的子元素（即嵌套对象）
                            var childChildElements = child.ChildNodes.Cast<XmlNode>()
                                .Where(n => n.NodeType == XmlNodeType.Element)
                                .ToList();
                            
                            if (childChildElements.Any())
                            {
                                // 这是一个嵌套对象，类型名就是元素名
                                properties[propertyName] = propertyName;
                            }
                            else
                            {
                                properties[propertyName] = GetDataTypeFromValue(child.InnerText.Trim());
                            }
                        }
                    }
                    else if (child.NodeType == XmlNodeType.Text || child.NodeType == XmlNodeType.CDATA)
                    {
                        // 如果元素有文本内容，也添加一个属性
                        if (!string.IsNullOrWhiteSpace(child.InnerText))
                        {
                            properties["Value"] = GetDataTypeFromValue(child.InnerText.Trim());
                        }
                    }
                }
            }

            return properties;
        }

        private static string GetDataTypeFromXml(XmlNode node)
        {
            // 如果有子元素，生成嵌套类
            var childElements = node.ChildNodes.Cast<XmlNode>()
                .Where(n => n.NodeType == XmlNodeType.Element)
                .ToList();

            if (childElements.Any())
            {
                // 返回嵌套类的类型名
                return ToPascalCase(node.Name);
            }
            else
            {
                // 没有子元素，根据文本内容判断类型
                if (!string.IsNullOrWhiteSpace(node.InnerText))
                {
                    return GetDataTypeFromValue(node.InnerText.Trim());
                }
                else
                {
                    return "string";
                }
            }
        }

        private static string GetDataTypeFromValue(string value)
        {
            // 尝试解析值的类型
            if (bool.TryParse(value, out _))
                return "bool";
            else if (int.TryParse(value, out _))
                return "int";
            else if (double.TryParse(value, out _))
                return "double";
            else if (DateTime.TryParse(value, out _))
                return "DateTime";
            else
                return "string";
        }

        private static string GenerateClassFromDictionary(Dictionary<string, object> dict, LanguageType language, string className)
        {
            var properties = new Dictionary<string, string>();
            var nestedClasses = new List<NestedClassInfo>();
            
            foreach (var kvp in dict)
            {
                string propertyName = ToPascalCase(kvp.Key);
                
                if (kvp.Value is Dictionary<string, object> nestedDict)
                {
                    // 这是一个嵌套对象，创建嵌套类
                    properties[propertyName] = propertyName; // 使用属性名作为类型名
                    nestedClasses.Add(new NestedClassInfo(propertyName, nestedDict));
                }
                else
                {
                    string propertyType = GetDataType(kvp.Value, language);
                    properties[propertyName] = propertyType;
                }
            }

            var sb = new StringBuilder();
            
            // 生成主类
            sb.Append(GenerateClassFromProperties(properties, language, className));
            
            // 生成嵌套类
            foreach (var nestedClass in nestedClasses)
            {
                var nestedProperties = new Dictionary<string, string>();
                
                foreach (var kvp in nestedClass.Data)
                {
                    string nestedPropertyName = ToPascalCase(kvp.Key);
                    string nestedPropertyType = GetDataType(kvp.Value, language);
                    nestedProperties[nestedPropertyName] = nestedPropertyType;
                }
                
                sb.AppendLine();
                sb.Append(GenerateClassFromProperties(nestedProperties, language, nestedClass.Name));
            }

            return sb.ToString();
        }

        private static string GetDataType(object value, LanguageType language)
        {
            if (value == null)
                return language == LanguageType.CSharp ? "object" : "Object";

            Type type = value.GetType();

            if (type == typeof(int) || type == typeof(long))
                return "int";
            else if (type == typeof(double) || type == typeof(float))
                return "double";
            else if (type == typeof(bool))
                return "bool";
            else if (type == typeof(string))
                return "string";
            else if (type == typeof(object[]))
            {
                object[] array = (object[])value;
                if (array.Length > 0)
                {
                    string elementType = GetDataType(array[0], language);
                    return language == LanguageType.CSharp ? $"List<{elementType}>" : elementType + "[]";
                }
                else
                {
                    return language == LanguageType.CSharp ? "List<object>" : "Object[]";
                }
            }
            else if (value is Dictionary<string, object> nestedDict)
            {
                // 嵌套对象 - 返回类名，需要进一步处理
                return "NestedObject";
            }
            else
                return "object";
        }

        private static string GenerateClassFromProperties(Dictionary<string, string> properties, LanguageType language, string className)
        {
            var sb = new StringBuilder();
            
            if (language == LanguageType.CSharp)
            {
                sb.AppendLine($"public class {className}");
                sb.AppendLine("{");
                
                foreach (var prop in properties)
                {
                    string propertyType = prop.Value;
                    sb.AppendLine($"    public {propertyType} {prop.Key} {{ get; set; }}");
                }
                
                sb.AppendLine("}");
            }
            else // Java
            {
                sb.AppendLine($"public class {className} {{");
                
                foreach (var prop in properties)
                {
                    string propertyType = prop.Value;
                    // 转换数据类型为Java类型
                    string javaType = ConvertToJavaType(propertyType);
                    sb.AppendLine($"    private {javaType} {ToCamelCase(prop.Key)};");
                }
                
                // 生成getter和setter方法
                foreach (var prop in properties)
                {
                    string propertyType = prop.Value;
                    string javaType = ConvertToJavaType(propertyType);
                    string camelCaseName = ToCamelCase(prop.Key);
                    string pascalCaseName = prop.Key;
                    
                    // Getter
                    sb.AppendLine($"    public {javaType} get{pascalCaseName}() {{");
                    sb.AppendLine($"        return {camelCaseName};");
                    sb.AppendLine("    }");
                    
                    // Setter
                    sb.AppendLine($"    public void set{pascalCaseName}({javaType} {camelCaseName}) {{");
                    sb.AppendLine($"        this.{camelCaseName} = {camelCaseName};");
                    sb.AppendLine("    }");
                }
                
                sb.AppendLine("}");
            }
            
            return sb.ToString();
        }

        private static string ConvertToJavaType(string csharpType)
        {
            switch (csharpType.ToLower())
            {
                case "int":
                    return "int";
                case "double":
                    return "double";
                case "bool":
                    return "boolean";
                case "string":
                    return "String";
                case "object":
                    return "Object";
                case "datetime":
                    return "Date";
                default:
                    // 处理List<T>类型
                    if (csharpType.StartsWith("List<"))
                    {
                        string elementType = csharpType.Substring(5, csharpType.Length - 6);
                        return $"List<{ConvertToJavaType(elementType)}>";
                    }
                    // 处理数组类型
                    else if (csharpType.EndsWith("[]"))
                    {
                        string elementType = csharpType.Substring(0, csharpType.Length - 2);
                        return $"{ConvertToJavaType(elementType)}[]";
                    }
                    else
                    {
                        // 假设是自定义类型
                        return csharpType;
                    }
            }
        }

        private static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // 移除特殊字符，只保留字母、数字和下划线
            input = Regex.Replace(input, @"[^\w]", "_");
            
            // 分割单词（基于下划线或驼峰命名法）
            var parts = Regex.Split(input, @"[_\s]+").Where(p => !string.IsNullOrEmpty(p));
            var result = new StringBuilder();

            foreach (var part in parts)
            {
                if (part.Length > 0)
                {
                    result.Append(char.ToUpper(part[0]));
                    if (part.Length > 1)
                        result.Append(part.Substring(1).ToLower());
                }
            }

            // 确保首字母大写
            return result.ToString();
        }

        private static string ToCamelCase(string input)
        {
            string pascal = ToPascalCase(input);
            if (string.IsNullOrEmpty(pascal))
                return pascal;
            
            if (pascal.Length == 1)
                return pascal.ToLower();
            
            return char.ToLower(pascal[0]) + pascal.Substring(1);
        }

        /// <summary>
        /// 确定输入是JSON还是XML并生成相应类
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="language">目标语言</param>
        /// <param name="className">类名</param>
        /// <returns>生成的类代码</returns>
        public static string GenerateClassFromInput(string input, LanguageType language, string className = "GeneratedClass")
        {
            if (string.IsNullOrWhiteSpace(input))
                return "// 输入为空";

            var trimmed = input.Trim();
            
            if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
            {
                // 看起来是JSON
                return GenerateClassFromJson(input, language, className);
            }
            else if (trimmed.StartsWith("<"))
            {
                // 看起来是XML
                return GenerateClassFromXml(input, language, className);
            }
            else
            {
                return "// 无法识别输入格式。请提供有效的JSON或XML。";
            }
        }
        
        // 定义一个简单的嵌套类信息类
        private class NestedClassInfo
        {
            public string Name { get; set; }
            public Dictionary<string, object> Data { get; set; }
            
            public NestedClassInfo(string name, Dictionary<string, object> data)
            {
                Name = name;
                Data = data;
            }
        }
    }
}