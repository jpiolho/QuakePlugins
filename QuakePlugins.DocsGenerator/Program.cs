using Grynwald.MarkdownGenerator;
using LoxSmoke.DocXml;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace QuakePlugins.DocsGenerator
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var xml = new XmlDocument();
            xml.Load(args[0]);

            var xmlMembers = xml.SelectSingleNode("doc/members");

            var apiGlobals = new SortedList<string, Type>();
            var apiTypes = new SortedList<string, Type>();
            foreach (var type in typeof(Quake).Assembly.GetTypes())
            {
                var doc = GetXmlDocumentForMember(xml, type, type);

                if (doc == null)
                    continue;

                if (doc.SelectSingleNode("apiglobal") != null)
                    apiGlobals.Add(type.Name, type);

                if (doc.SelectSingleNode("apitype") != null)
                    apiTypes.Add(type.Name, type);
            }

            await Task.WhenAll(
                GenerateGlobalsDocAsync(xml, apiGlobals.Values)
                ,GenerateTypesDocsAsync(xml, apiTypes.Values)
            );
        }

        private static async Task GenerateGlobalsDocAsync(XmlDocument xml, IEnumerable<Type> globals)
        {
            var md = new MdDocument();

            md.Root.Add(new MdHeading("Globals", 1));
            md.Root.Add(new MdParagraph("This is a list of all the globals available on the API"));

            md.Root.Add(new MdBulletList(globals.Select(a => new MdListItem(new MdLinkSpan(a.Name, $"#{a.Name}")))));

            foreach (var global in globals)
            {
                md.Root.Add(new MdHeading(global.Name, 2));
                var block = new MdContainerBlock();

                await AppendClassDocAsync(block, xml, global);

                md.Root.Add(block);
            }

            await File.WriteAllTextAsync("globals.md", md.ToString());
        }

        private static async Task GenerateTypesDocsAsync(XmlDocument xml, IEnumerable<Type> types)
        {
            var md = new MdDocument();

            md.Root.Add(new MdHeading("Types", 1));
            md.Root.Add(new MdParagraph("This is a list of all the types available on the API"));

            md.Root.Add(new MdBulletList(types.Select(a => new MdListItem(new MdLinkSpan(a.Name, $"#{a.Name}")))));

            foreach (var type in types)
            {
                md.Root.Add(new MdHeading(type.Name, 2));
                var block = new MdContainerBlock();

                await AppendClassDocAsync(block, xml, type);

                md.Root.Add(block);
            }

            await File.WriteAllTextAsync("types.md", md.ToString());
        }

        private static async Task AppendClassDocAsync(MdContainerBlock container, XmlDocument xml, Type type)
        {
            var links = new MdContainerBlock();
            var linksTable = new MdBulletList();

            var blockMethods = new MdContainerBlock();
            foreach (var method in type.GetMethods())
            {
                // Ignore private methods
                if (!method.IsPublic)
                    continue;

                // Ignore getter / setters
                if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
                    continue;

                // Ignore the default class methods
                switch (method.Name.ToUpperInvariant())
                {
                    case "GETTYPE":
                    case "TOSTRING":
                    case "EQUALS":
                    case "GETHASHCODE":
                        continue;
                }

                blockMethods.Add(new MdHeading(method.Name, 3));

                var doc = GetXmlDocumentForMember(xml, method);

                if (doc != null)
                {
                    var summary = doc.SelectSingleNode("summary");
                    if (summary != null)
                        blockMethods.Add(new MdParagraph(summary.InnerText.Trim()));
                }

                var parameters = new StringBuilder();
                foreach (var parameter in method.GetParameters())
                {
                    if (parameters.Length > 0)
                        parameters.Append(", ");

                    parameters.Append(GetTypeName(parameter.ParameterType));
                    parameters.Append(" ");
                    parameters.Append(parameter.Name);

                    if (parameter.IsOptional)
                    {
                        parameters.Append(" = ");
                        parameters.Append(GetParameterDefaultValue(parameter));
                    }

                }

                blockMethods.Add(new MdCodeBlock($"{GetTypeName(method.ReturnType)} {method.Name}({parameters})", "csharp"));
            }

            // Generate docs for properties
            var blockProperties = new MdContainerBlock();

            foreach(var property in type.GetProperties())
            {
                if (property.GetGetMethod() == null)
                    continue;

                var hasSetMethod = property.GetSetMethod() != null;

                var block = new MdContainerBlock();

                if(hasSetMethod)
                    block.Add(new MdHeading(property.Name, 4));
                else
                    block.Add(new MdHeading(property.Name + " (Read-only)", 3));

                block.Add(new MdParagraph("Type: ",new MdCodeSpan(GetTypeName(property.PropertyType))));

                var doc = GetXmlDocumentForMember(xml, property);
                if (doc != null)
                {
                    var summary = doc.SelectSingleNode("summary");

                    if (summary != null)
                        block.Add(new MdParagraph(summary.InnerText.Trim()));
                }


                if(blockProperties.Count == 0)
                    blockProperties.Add(new MdHeading("Properties", 3));

                blockProperties.Add(block);
            }

            container.Add(blockMethods);
            container.Add(blockProperties);
        }

        private static string GetTypeName(Type type)
        {
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return GetTypeName(type.GetGenericArguments()[0]) + "?";
            }
            else if (type == typeof(float)) return "Float";
            else if (type == typeof(int)) return "Int";
            else if (type == typeof(uint)) return "Uint";
            else if (type == typeof(bool)) return "Bool";


            return type.Name;
        }

        private static string GetParameterDefaultValue(ParameterInfo info)
        {
            var raw = info.RawDefaultValue;

            if (raw == null)
                return "null";

            if (info.ParameterType == typeof(string))
                return $"\"{raw}\"";

            return raw.ToString();
        }

        private static XmlElement? GetXmlDocumentForMember(XmlDocument xml, MemberInfo info, Type? type = null)
        {
            string? id = null;

            switch (info.MemberType)
            {
                case MemberTypes.TypeInfo: id = XmlDocId.TypeId(type); break;
                default: id = XmlDocId.MemberId(info); break;
            }

            if (id == null)
                return null;

            return (XmlElement?)xml.SelectSingleNode($"doc/members/member[@name='{id}']");
        }
    }
}