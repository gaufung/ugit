namespace Tindo.UgitCLI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    internal static class Extensions
    {
        public static ParserResult<object> ParseArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12,
            T13, T14, T15, T16, T17>(this Parser parser, IEnumerable<string> args)
        {
            if (parser == null)
            {
                throw new ArgumentNullException("parser");
            }

            return parser.ParseArguments(args, new[]
            {
                typeof(T1),
                typeof(T2),
                typeof(T3),
                typeof(T4),
                typeof(T5),
                typeof(T6),
                typeof(T7),
                typeof(T8),
                typeof(T9),
                typeof(T10),
                typeof(T11),
                typeof(T12),
                typeof(T13),
                typeof(T14),
                typeof(T15),
                typeof(T16),
                typeof(T17),
            });
        }

        [ExcludeFromCodeCoverage]
        public static TResult MapResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17,
            TResult>(
            this ParserResult<object> result,
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
            if (result is Parsed<object> parsed)
            {
                if (parsed.Value is T1 t1)
                {
                    return parsedFunc1(t1);
                }

                if (parsed.Value is T2 t2)
                {
                    return parsedFunc2(t2);
                }

                if (parsed.Value is T3 t3)
                {
                    return parsedFunc3(t3);
                }

                if (parsed.Value is T4 t4)
                {
                    return parsedFunc4(t4);
                }

                if (parsed.Value is T5 t5)
                {
                    return parsedFunc5(t5);
                }

                if (parsed.Value is T6 t6)
                {
                    return parsedFunc6(t6);
                }

                if (parsed.Value is T7 t7)
                {
                    return parsedFunc7(t7);
                }

                if (parsed.Value is T8 t8)
                {
                    return parsedFunc8(t8);
                }

                if (parsed.Value is T9 t9)
                {
                    return parsedFunc9(t9);
                }

                if (parsed.Value is T10 t10)
                {
                    return parsedFunc10(t10);
                }

                if (parsed.Value is T11 t11)
                {
                    return parsedFunc11(t11);
                }

                if (parsed.Value is T12 t12)
                {
                    return parsedFunc12(t12);
                }

                if (parsed.Value is T13 t13)
                {
                    return parsedFunc13(t13);
                }

                if (parsed.Value is T14 t14)
                {
                    return parsedFunc14(t14);
                }

                if (parsed.Value is T15 t15)
                {
                    return parsedFunc15(t15);
                }

                if (parsed.Value is T16 t16)
                {
                    return parsedFunc16(t16);
                }

                if (parsed.Value is T17 t17)
                {
                    return parsedFunc17(t17);
                }

                throw new InvalidOperationException();
            }

            return notParsedFunc(((NotParsed<object>) result).Errors);
        }

        [ExcludeFromCodeCoverage]
        public static ParserResult<object> ParseArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14,
            T15, T16, T17, T18, T19>(this Parser parser, IEnumerable<string> args)
        {
            if (parser == null)
            {
                throw new ArgumentNullException("parser");
            }

            return parser.ParseArguments(args, new[]
            {
                typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
                typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13),
                typeof(T14), typeof(T15), typeof(T16), typeof(T17), typeof(T18), typeof(T19)
            });
        }

        [ExcludeFromCodeCoverage]
        public static TResult MapResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18,T19,
            TResult>(
            this ParserResult<object> result,
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
            Func<T18, TResult> parsedFunc18,
            Func<T19, TResult> parsedFunc19,
            Func<IEnumerable<Error>, TResult> notParsedFunc)
        {
            if (result is Parsed<object> parsed)
            {
                if (parsed.Value is T1 t1)
                {
                    return parsedFunc1(t1);
                }

                if (parsed.Value is T2 t2)
                {
                    return parsedFunc2(t2);
                }

                if (parsed.Value is T3 t3)
                {
                    return parsedFunc3(t3);
                }

                if (parsed.Value is T4 t4)
                {
                    return parsedFunc4(t4);
                }

                if (parsed.Value is T5 t5)
                {
                    return parsedFunc5(t5);
                }

                if (parsed.Value is T6 t6)
                {
                    return parsedFunc6(t6);
                }

                if (parsed.Value is T7 t7)
                {
                    return parsedFunc7(t7);
                }

                if (parsed.Value is T8 t8)
                {
                    return parsedFunc8(t8);
                }

                if (parsed.Value is T9 t9)
                {
                    return parsedFunc9(t9);
                }

                if (parsed.Value is T10 t10)
                {
                    return parsedFunc10(t10);
                }

                if (parsed.Value is T11 t11)
                {
                    return parsedFunc11(t11);
                }

                if (parsed.Value is T12 t12)
                {
                    return parsedFunc12(t12);
                }

                if (parsed.Value is T13 t13)
                {
                    return parsedFunc13(t13);
                }

                if (parsed.Value is T14 t14)
                {
                    return parsedFunc14(t14);
                }

                if (parsed.Value is T15 t15)
                {
                    return parsedFunc15(t15);
                }

                if (parsed.Value is T16 t16)
                {
                    return parsedFunc16(t16);
                }

                if (parsed.Value is T17 t17)
                {
                    return parsedFunc17(t17);
                }

                if (parsed.Value is T18 t18)
                {
                    return parsedFunc18(t18);
                }

                if (parsed.Value is T19 t19)
                {
                    return parsedFunc19(t19);
                }
                

                throw new InvalidOperationException();
            }

            return notParsedFunc(((NotParsed<object>) result).Errors);
        }
        
        [ExcludeFromCodeCoverage]
        public static ParserResult<object> ParseArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14,
            T15, T16, T17, T18>(this Parser parser, IEnumerable<string> args)
        {
            if (parser == null)
            {
                throw new ArgumentNullException("parser");
            }

            return parser.ParseArguments(args, new[]
            {
                typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8),
                typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13),
                typeof(T14), typeof(T15), typeof(T16), typeof(T17), typeof(T18),
            });
        }

        [ExcludeFromCodeCoverage]
        public static TResult MapResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18,
            TResult>(
            this ParserResult<object> result,
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
            Func<T18, TResult> parsedFunc18,
            Func<IEnumerable<Error>, TResult> notParsedFunc)
        {
            if (result is Parsed<object> parsed)
            {
                if (parsed.Value is T1 t1)
                {
                    return parsedFunc1(t1);
                }

                if (parsed.Value is T2 t2)
                {
                    return parsedFunc2(t2);
                }

                if (parsed.Value is T3 t3)
                {
                    return parsedFunc3(t3);
                }

                if (parsed.Value is T4 t4)
                {
                    return parsedFunc4(t4);
                }

                if (parsed.Value is T5 t5)
                {
                    return parsedFunc5(t5);
                }

                if (parsed.Value is T6 t6)
                {
                    return parsedFunc6(t6);
                }

                if (parsed.Value is T7 t7)
                {
                    return parsedFunc7(t7);
                }

                if (parsed.Value is T8 t8)
                {
                    return parsedFunc8(t8);
                }

                if (parsed.Value is T9 t9)
                {
                    return parsedFunc9(t9);
                }

                if (parsed.Value is T10 t10)
                {
                    return parsedFunc10(t10);
                }

                if (parsed.Value is T11 t11)
                {
                    return parsedFunc11(t11);
                }

                if (parsed.Value is T12 t12)
                {
                    return parsedFunc12(t12);
                }

                if (parsed.Value is T13 t13)
                {
                    return parsedFunc13(t13);
                }

                if (parsed.Value is T14 t14)
                {
                    return parsedFunc14(t14);
                }

                if (parsed.Value is T15 t15)
                {
                    return parsedFunc15(t15);
                }

                if (parsed.Value is T16 t16)
                {
                    return parsedFunc16(t16);
                }

                if (parsed.Value is T17 t17)
                {
                    return parsedFunc17(t17);
                }

                if (parsed.Value is T18 t18)
                {
                    return parsedFunc18(t18);
                }

                throw new InvalidOperationException();
            }

            return notParsedFunc(((NotParsed<object>) result).Errors);
        }
    }
}