using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;

namespace ugit
{
    public static class Extensions
    {
        public static byte[] Encode(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string Decode(this byte[] arr)
        {
            return Encoding.UTF8.GetString(arr);
        }

        public static string Sha1HexDigest(this byte[] data)
        {
            using var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(data);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public static void CreateParentDirectory(this string filePath, IFileSystem fileSystem)
        {
            string folder = fileSystem.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(folder) && !fileSystem.Directory.Exists(folder))
            {
                fileSystem.Directory.CreateDirectory(folder);
            }
        }
        
        public static void Update<TKey, TValue>(this IDictionary<TKey, TValue> left, IDictionary<TKey, TValue> right)
        {
            foreach (var entry in right)
            {
                left[entry.Key] = entry.Value;
            }
        }
        
        public static bool IsOnlyHex(this string str)
        {
            return Regex.IsMatch(str, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        public static IEnumerable<string> Walk(this IFileSystem fileSystem, string directory)
        {
            List<string> filePaths =new List<string>();
            foreach (var filePath in fileSystem.Directory.EnumerateFiles(directory))
            {
                filePaths.Add(filePath);
            }
            
            foreach (var directoryPath in fileSystem.Directory.EnumerateDirectories(directory))
            {
                var subFilePaths = fileSystem.Walk(directoryPath);
                filePaths.AddRange(subFilePaths);
            }

            return filePaths;
        }
        
        public static T Pop<T>(this HashSet<T> set)
        {
            var item = set.First();
            set.Remove(item);
            return item;
        }
        
        public static ParserResult<object> ParseArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12
        , T13, T14, T15, T16, T17>(this Parser parser, IEnumerable<string> args)
        {
            if (parser == null) throw new ArgumentNullException("parser");

            return parser.ParseArguments(args, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
                typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13),
                typeof(T14),typeof(T15),typeof(T16),typeof(T17)
            });
        }
        
        public static TResult MapResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, TResult>(this ParserResult<object> result,
            Func<T1, TResult> parsedFunc1,
            Func<T2, TResult> parsedFunc2,
            Func<T3, TResult> parsedFunc3,
            Func<T4, TResult> parsedFunc4,
            Func<T5, TResult> parsedFunc5,
            Func<T6, TResult> parsedFunc6,
            Func<T7, TResult> parsedFunc7,
            Func<T8, TResult> parsedFunc8,
            Func<T9, TResult> parsedFunc9,
            Func<T10, TResult> parsedFunc10,
            Func<T11, TResult> parsedFunc11,
            Func<T12, TResult> parsedFunc12,
            Func<T13, TResult> parsedFunc13,
            Func<T14, TResult> parsedFunc14,
            Func<T15, TResult> parsedFunc15,
            Func<T16, TResult> parsedFunc16,
            Func<T17, TResult> parsedFunc17,
            Func<IEnumerable<Error>, TResult> notParsedFunc)
        {
            var parsed = result as Parsed<object>;
            if (parsed != null)
            {
                if (parsed.Value is T1)
                {
                    return parsedFunc1((T1)parsed.Value);
                }
                if (parsed.Value is T2)
                {
                    return parsedFunc2((T2)parsed.Value);
                }
                if (parsed.Value is T3)
                {
                    return parsedFunc3((T3)parsed.Value);
                }
                if (parsed.Value is T4)
                {
                    return parsedFunc4((T4)parsed.Value);
                }
                if (parsed.Value is T5)
                {
                    return parsedFunc5((T5)parsed.Value);
                }
                if (parsed.Value is T6)
                {
                    return parsedFunc6((T6)parsed.Value);
                }
                if (parsed.Value is T7)
                {
                    return parsedFunc7((T7)parsed.Value);
                }
                if (parsed.Value is T8)
                {
                    return parsedFunc8((T8)parsed.Value);
                }
                if (parsed.Value is T9)
                {
                    return parsedFunc9((T9)parsed.Value);
                }
                if (parsed.Value is T10)
                {
                    return parsedFunc10((T10)parsed.Value);
                }
                if (parsed.Value is T11)
                {
                    return parsedFunc11((T11)parsed.Value);
                }
                if (parsed.Value is T12)
                {
                    return parsedFunc12((T12)parsed.Value);
                }
                if (parsed.Value is T13)
                {
                    return parsedFunc13((T13)parsed.Value);
                }
                if (parsed.Value is T14)
                {
                    return parsedFunc14((T14)parsed.Value);
                }
                if (parsed.Value is T15)
                {
                    return parsedFunc15((T15)parsed.Value);
                }
                if (parsed.Value is T16)
                {
                    return parsedFunc16((T16)parsed.Value);
                }

                if (parsed.Value is T17)
                {
                    return parsedFunc17((T17) parsed.Value);
                }
                throw new InvalidOperationException();
            }
            return notParsedFunc(((NotParsed<object>)result).Errors);
        }
    }
}