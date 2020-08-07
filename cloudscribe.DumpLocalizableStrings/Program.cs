#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CommandLine;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

#endregion

namespace cloudscribe.DumpLocalizableStrings
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string input = "";
            string output = "";

            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed(o =>
                              {
                                  input = o.Input;
                                  Console.WriteLine($"Input Dir: {input}");

                                  output = string.IsNullOrWhiteSpace(o.Output) ? AppDomain.CurrentDomain.BaseDirectory : o.Output;
                                  Console.WriteLine($"Output Dir: {output}");
                              });

            try
            {
                if (Directory.Exists(input))
                {
                    Dictionary<string, HashSet<string>> resultDict = new Dictionary<string, HashSet<string>>();
                    string[] files = Directory.GetFiles(input, "*.dll", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        using (ModuleDefMD module = ModuleDefMD.Load(file))
                        {
                            foreach (TypeDef type in module.GetTypes())
                            {
                                foreach (MethodDef method in type.Methods)
                                {
                                    if (!method.HasBody)
                                    {
                                        continue;
                                    }

                                    bool localizable = false;
                                    string operandType = "";

                                    foreach (Instruction instr in method.Body.Instructions)
                                    {
                                        if (instr.OpCode == OpCodes.Call && instr.Operand.ToString().IndexOf("IStringLocalizer`1", StringComparison.InvariantCulture) >= 0)
                                        {
                                            MethodDef md = instr.Operand as MethodDef;
                                            if (md?.ReturnType != null && md.ReturnType.IsGenericInstanceType)
                                            {
                                                GenericInstSig sig = md.ReturnType as GenericInstSig;
                                                if (sig?.GenericArguments != null && sig.GenericArguments.Count > 0)
                                                {
                                                    localizable = true;
                                                    operandType = sig.GenericArguments[0].TypeName;
                                                }
                                            }

                                            continue;
                                        }

                                        if (instr.OpCode == OpCodes.Ldstr && localizable && !string.IsNullOrEmpty(operandType))
                                        {
                                            HashSet<string> resources = resultDict.GetOrAdd(operandType, s => new HashSet<string>());
                                            string value = WebUtility.HtmlEncode((string) instr.Operand);
                                            if (!string.IsNullOrWhiteSpace(value))
                                            {
                                                resources.Add(value);
                                                Console.WriteLine($"Resource: {operandType}{Environment.NewLine}Value: {value}");
                                                Console.WriteLine("===========================================================");
                                            }
                                        }

                                        localizable = false;
                                        operandType = "";
                                    }
                                }
                            }
                        }

                        if (!Directory.Exists(output))
                        {
                            Directory.CreateDirectory(output);
                        }

                        string resxTemplate = ReadResource("ResxTemplate.xml");
                        string resxTemplateItem = ReadResource("ResxTemplateItem.xml");

                        foreach (KeyValuePair<string, HashSet<string>> keyValuePair in resultDict)
                        {
                            StringBuilder builder = new StringBuilder();
                            foreach (string str in keyValuePair.Value)
                            {
                                builder.AppendLine(string.Format(resxTemplateItem, str, str));
                            }

                            string resxFile = Path.Combine(output, $"{keyValuePair.Key}.en-US.resx");
                            if (File.Exists(resxFile))
                            {
                                File.Delete(resxFile);
                            }

                            //File.WriteAllText(resxFile, PrettyXml(string.Format(resxTemplate, builder)));
                            File.WriteAllText(resxFile, string.Format(resxTemplate, builder.ToString().TrimEnd(Environment.NewLine.ToCharArray())));
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Input Directory '{input}' does not exists!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.ReadKey();
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static string ReadResource(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException()))
            {
                return reader.ReadToEnd();
            }
        }

        private static string PrettyXml(string xml)
        {
            StringBuilder stringBuilder = new StringBuilder();
            XElement element = XElement.Parse(xml);
            XmlWriterSettings settings = new XmlWriterSettings {OmitXmlDeclaration = false, Indent = true, NewLineOnAttributes = false, Encoding = Encoding.UTF8};

            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                element.Save(xmlWriter);
            }

            return stringBuilder.ToString();
        }
    }
}
