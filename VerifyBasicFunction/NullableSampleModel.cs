using System;
using System.Collections.Generic;

namespace VerifyBasicFunction
{
    /// <summary>Nullableに関してテストするためのサンプルモデル</summary>
    public class NullableSampleModel
    {
        /// <summary>ヌル許容Int</summary>
        public int? NullableNumber { get; set; }

        /// <summary>ヌル非許容Int</summary>
        public int NormalNumber { get; set; }

        // .NET Standard 2.0 & C#7.3ではヌル許容型を使用できない。
        /// <summary>ヌル許容String</summary>
        public string? NullableString { get; set; }

        /// <summary>ヌル非許容String</summary>
        public string NormalString { get; set; }

        // .NET Standard 2.0 & C#7.3ではヌル許容型を使用できない。
        /// <summary>ヌル許容リスト</summary>
        public List<int>? NullableList { get; set; }

        /// <summary>ヌル非許容リスト</summary>
        public List<int> NormalList { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullableSampleModel"/> class.
        /// </summary>
        public NullableSampleModel() :
            this(null, 0, null, string.Empty, null, new List<int>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NullableSampleModel"/> class.
        /// </summary>
        /// <param name="nullableNumber">The nullable number.</param>
        /// <param name="normalNumber">The normal number.</param>
        /// <param name="nullableString">The nullable string.</param>
        /// <param name="normalString">The normal string.</param>
        /// <param name="nullableList">The nullable list.</param>
        /// <param name="normalList">The normal list.</param>
        public NullableSampleModel(
            int? nullableNumber,
            int normalNumber,
            // .NET Standard 2.0 & C#7.3ではヌル許容型を使用できない。
            string? nullableString,
            string normalString,
            // .NET Standard 2.0 & C#7.3ではヌル許容型を使用できない。
            List<int>? nullableList,
            List<int> normalList)
        {
            NullableNumber = nullableNumber;
            NormalNumber = normalNumber;
            NullableString = nullableString;
            NormalString = normalString;
            NullableList = nullableList;
            NormalList = normalList;
        }

        /// <summary>
        /// Null代入にまつわるエラー・警告の検証のための関数。
        /// </summary>
        public static void SugstituteNull()
        {
            var nullableSampleModel = new NullableSampleModel();
            if (nullableSampleModel.NullableNumber == null &&
                nullableSampleModel.NullableString == null &&
                nullableSampleModel.NullableList == null)
            {
                Console.WriteLine("Null is null.");
            }

            if (nullableSampleModel.NormalNumber == 0 &&
                nullableSampleModel.NormalString == string.Empty &&
                nullableSampleModel.NormalList.Count == 0
                )
            {
                Console.WriteLine("Non Null is empty.");
            }

            // CS0037  Null 非許容の値型であるため、Null を 'int' に変換できません
            //int num = null;

            // OK
            int? nullableNum = null;
            if (nullableNum == null) { Console.WriteLine("nullableNum is null."); }

            // 次の警告は.NET Standard 2.0 & C#7.3では出ない。
            // 警告  CS8600 Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
            // string str = null;
            string? str = null;
            if (str == null) { Console.WriteLine("str is null."); }
            // .NET Standard 2.0 & C#7.3ではヌル許容型を使用できない。
            string? nullableStr = null;
            if (nullableStr == null) { Console.WriteLine("nullableStr is null."); }

            // 次の警告は.NET Standard 2.0 & C#7.3では出ない。
            // 警告  CS8600 Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
            // List<int> list = null;
            List<int>? list = null;
            if (list == null) { Console.WriteLine("list is null."); }
            // .NET Standard 2.0 & C#7.3ではヌル許容型を使用できない。
            List<int>? nullableList = null;
            if (nullableList == null) { Console.WriteLine("nullableList is null."); }

            // 次の警告は.NET Standard 2.0 & C#7.3では出ない。
            // 警告  CS8600 Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
            //string message = null;
            string? message = null;

            // 次の警告は.NET Standard 2.0 & C#7.3では出ない。
            // 警告  CS8602  null 参照の可能性があるものの逆参照です。
            // Console.WriteLine($"The length of the message is {message.Length}");
            // null条件演算子を使用することで警告を回避できる。
            Console.WriteLine($"The length of the message is {message?.Length}");

            var originalMessage = message;
            message = "Hello, World!";

            // No warning. Analysis determined "message" is not-null.
            Console.WriteLine($"The length of the message is {message.Length}");

            // warning!
            // Console.WriteLine(originalMessage.Length);
            // null条件演算子を使用することで警告を回避できる。
            Console.WriteLine(originalMessage?.Length);
        }
    }
}
