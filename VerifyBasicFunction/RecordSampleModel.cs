using System.Collections.Generic;
using System.Collections.Immutable;

namespace VerifyBasicFunction
{
    // .NET Standard 2.0ではrecordは使えない。次のエラーが発生する。
    // エラー CS0246  型または名前空間の名前 'record' が見つかりませんでした
    public record RecordSampleModel
    {
        // .NET Standard 2.0ではinitは使えない。次のエラーが発生する。
        // エラー CS8370  機能 'init 専用セッター' は C# 7.3 では使用できません。9.0 以上の言語バージョンをお使いください。
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the age.
        /// </summary>
        public int Age { get; init; }

        /// <summary>
        /// Gets the graduate year.
        /// </summary>
        public List<int> GraduateYear { get; init; }

        /// <summary>
        /// Gets the immutable graduate year.
        /// </summary>
        public ImmutableList<int> ImmutableGraduateYear { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordSampleModel"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="age">The age.</param>
        /// <param name="graduateYear">The graduate year.</param>
        /// <param name="immutableGraduateYear">The immutable graduate year.</param>
        public RecordSampleModel(
            string name,
            int age,
            List<int> graduateYear,
            ImmutableList<int> immutableGraduateYear)
        {
            Name = name;
            Age = age;
            GraduateYear = graduateYear;
            ImmutableGraduateYear = immutableGraduateYear;
        }
    }
}
