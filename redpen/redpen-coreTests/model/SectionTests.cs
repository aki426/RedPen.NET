using FluentAssertions;
using Xunit;

namespace redpen_core.model.Tests
{
    public class SectionTests
    {
        [Fact()]
        public void GetJoinedHeaderContentsTest()
        {
            Section section;

            // Headerが1つの場合
            section = new Section(0, "header string");

            section.GetJoinedHeaderContents().Content.Should().Be("header string");
            section.GetJoinedHeaderContents().LineNumber.Should().Be(0);

            // 複数SentenceHeaderの場合
            List<Sentence> headers = new List<Sentence>()
            {
                new Sentence("header1.", 0),
                new Sentence("header2.", 0),
                new Sentence("header3.", 0),
            };

            section = new Section(0, headers);
            section.GetJoinedHeaderContents().Content.Should().Be("header1. header2. header3.");
            section.GetJoinedHeaderContents().LineNumber.Should().Be(0);

            // Headerがない場合
            section = new Section(0);
            section.GetJoinedHeaderContents().Content.Should().Be("");
            section.GetJoinedHeaderContents().LineNumber.Should().Be(0);
        }
    }
}
