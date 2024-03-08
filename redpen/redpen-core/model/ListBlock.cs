namespace redpen_core.model
{
    public record class ListBlock
    {
        public List<ListElement> ListElements { get; init; }

        public ListBlock()
        {
            this.ListElements = new List<ListElement>();
        }

        public int GetNumberOfListElements()
        {
            return this.ListElements.Count;
        }

        public ListElement GetListElement(int index)
        {
            return this.ListElements[index];
        }

        /// <summary>
        /// Append ListElement.
        /// </summary>
        /// <param name="contents">contents of list element</param>
        /// <param name="level">indentation level</param>
        public void AppendListElement(List<Sentence> contents, int level)
        {
            this.ListElements.Add(new ListElement(contents, level));
        }
    }
}
