﻿using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using HtmlAgilityPack;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections;
using Newtonsoft.Json;
using MyConsole;
using System.Globalization;
using System.Threading.Tasks;

namespace MTBReportParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            var operations = new List<Operation>();
            operations.AddRange(Load());
            Console.WriteLine(string.Join(Environment.NewLine, operations.Where(_ => _.Status != Status.Decline).Select(_ => _.ToString())));

            var console = new ListeningUserInput();
            var help = "help";
            var exit = "exit";
            while (true)
            {
                var input = console.Listen();

                if (input == help)
                {
                    Console.WriteLine($"{Environment.NewLine}{new string('*', Console.WindowWidth - 1)}");
                    ;
                    Console.WriteLine(
                        $"Use '_' to access collection." +
                        $"{Environment.NewLine}Last item as json:" +
                        $"{Environment.NewLine}{JsonConvert.SerializeObject(operations.Last(), Formatting.Indented)}" +
                        $"{Environment.NewLine}Types:" +
                        $"{Environment.NewLine}{string.Join(Environment.NewLine, typeof(Operation).GetProperties(System.Reflection.BindingFlags.Public| System.Reflection.BindingFlags.Instance).Select(_ => $"{_.PropertyType} {_.Name}"))}");
                    Console.WriteLine($"{new string('*', Console.WindowWidth-1)}{Environment.NewLine}");
                }
                else if (input == exit)
                {
                    Console.WriteLine("Bye");
                    Task.Delay(300).Wait();
                    break;
                }
                else
                {
                    try
                    {
                        var scriptOptions = ScriptOptions.Default.AddImports("System.Linq").AddImports("System").AddReferences(typeof(Enumerable).Assembly);
                        var result = CSharpScript.EvaluateAsync(
                                   input,
                                   scriptOptions, globals: new Global() { _ = operations }).Result;


                        if (typeof(IEnumerable).IsAssignableFrom(result.GetType()))
                        {
                            Console.WriteLine(string.Join(Environment.NewLine, (result as IEnumerable<object>).Select(_ => _.ToString())));
                        }
                        else
                        {
                            Console.WriteLine(result);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine($"Input '{help}' to get more info.");
                    }
                }
            }
        }

      

        private static HtmlNode OpenNode(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode;
        }

        private static Status GetStatus(object complete, object notCompleted, object operationIn)
        {
            if (complete != null && notCompleted == null && operationIn == null)
            {
                return Status.Success;
            }
            else if (complete == null && notCompleted != null && operationIn == null)
            {
                return Status.Decline;
            }
            else if (complete == null && notCompleted == null && operationIn != null)
            {
                return Status.InProgress;
            }
            else
            {
                return Status.Unknown;
            }
        }

        private static List<Operation> Load()
        {
            var result = new List<Operation>();
            var files = Directory.GetFiles(ConfigurationManager.AppSettings["Folder"]);

            for (int i = 0; i < files.Length; i++)
            {
                double percent = (i + 1) / (double)files.Length * 100;
                Console.Write($"\rLoading {percent.ToString("F0")}%");

                var raw = File.ReadAllText(files[i]);
                var days = OpenNode(raw).SelectNodes("//div[contains(@class, 'operations-by-day-row payment-history')]").ToList();

                var listNodes = new List<Operation>();

                foreach (var node in days.Select(_ => OpenNode(_.InnerHtml)))
                {
                    var date = node.SelectSingleNode("//div[contains(@class, 'day-header')]").InnerText;
                    date = date.Contains("Сегодня") ? date.Replace("Сегодня", "") : date;
                    var dayOperations = node.SelectNodes("//div[contains(@class, 'operation-row-wrap')]");

                    foreach (var operation in dayOperations.Select(_ => OpenNode(_.InnerHtml)))
                    {
                        var complete = operation.SelectSingleNode("//div[contains(@class, 'operation-status complete')]");
                        var notCompleted = operation.SelectSingleNode("//div[contains(@class, 'operation-status operation-not-completed')]");
                        var operationIn = operation.SelectSingleNode("//div[contains(@class, 'operation-status operation-in')]");
                        var status = GetStatus(complete, notCompleted, operationIn);
                        var title = operation.SelectNodes("//div[contains(@class, 'operation-title')]")[1].InnerText;
                        var place = operation.SelectSingleNode("//div[contains(@class, 'operation-place')]").InnerText;
                        var time = operation.SelectSingleNode("//div[contains(@class, 'operation-time')]").InnerText;
                        var currencyNode = operation.SelectSingleNode("//div[contains(@class, 'operation-sum-currency-main')]");
                        var amount = currencyNode != null ? double.Parse($"{currencyNode.ChildNodes[0].InnerText}{currencyNode.ChildNodes[1].InnerText}".Replace(" ", "").Replace(",", ".")) : 0;
                        var currency = currencyNode?.ChildNodes[2]?.InnerText;

                        var dateTime = DateTime.Parse($"{date.Replace(",", "").Replace("года", "")} {time}", new CultureInfo("ru-RU"));
                        listNodes.Add(new Operation() { DateTime = dateTime, Title = title, Place = place, Amount = amount, Currency = currency, Status = status });
                    }
                }

                listNodes.Reverse();
                result.AddRange(listNodes);
            }
            Console.WriteLine();

            return result;
        }
    }

}
