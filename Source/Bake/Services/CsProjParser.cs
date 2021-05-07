using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Bake.ValueObjects.Recipes.DotNet;

namespace Bake.Services
{
    public class CsProjParser : ICsProjParser
    {
        public async Task<CsProj> ParseAsync(
            string path,
            CancellationToken cancellationToken)
        {
            var xml = await File.ReadAllTextAsync(
                path,
                cancellationToken);
            var xDocument = XDocument.Parse(xml);

            var packAsTool = ReadBool(xDocument ,"/Project/PropertyGroup/IsTool");
            var isPackable = ReadBool(xDocument, "/Project/PropertyGroup/IsPackable");
            var isPublishable = ReadBool(xDocument, "/Project/PropertyGroup/IsPublishable");
            var toolCommandName = ReadString(xDocument ,"/Project/PropertyGroup/ToolCommandName");

            return new CsProj(
                packAsTool,
                toolCommandName,
                isPackable,
                isPublishable);
        }

        private static string ReadString(
            XDocument xDocument,
            string xPath)
        {
            return xDocument.XPathSelectElement(xPath)?.Value ?? string.Empty;
        }

        private static bool ReadBool(
            XDocument xDocument,
            string xPath,
            bool defaultValue = false)
        {
            var value = xDocument.XPathSelectElement(xPath)?.Value;
            return bool.TryParse(value, out var b)
                ? b
                : defaultValue;
        }
    }
}
